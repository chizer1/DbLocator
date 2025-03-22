using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers;

internal record UpdateDatabaseUserCommand(
    int DatabaseUserId,
    string UserName,
    string UserPassword,
    bool UpdateDatabase = false
);

internal sealed class UpdateDatabaseUserCommandValidator
    : AbstractValidator<UpdateDatabaseUserCommand>
{
    internal UpdateDatabaseUserCommandValidator()
    {
        RuleFor(x => x.DatabaseUserId).NotNull().WithMessage("DatabaseUser Id is required.");

        RuleFor(x => x.UserName)
            .MaximumLength(50)
            .WithMessage("DatabaseUserName cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("DatabaseUserName can only contain letters, numbers, and underscores.");

        RuleFor(x => x.UserPassword)
            .MinimumLength(8)
            .WithMessage("DatabaseUserPassword must be at least 8 characters long.")
            .Matches(@"[A-Z]")
            .WithMessage("DatabaseUserPassword must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage("DatabaseUserPassword must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
            .WithMessage("DatabaseUserPassword must contain at least one number.")
            .Matches(@"[\W_]")
            .WithMessage("DatabaseUserPassword must contain at least one special character.")
            .MaximumLength(50)
            .WithMessage("DatabaseUserPassword cannot be more than 50 characters.");
    }
}

internal class UpdateDatabaseUser(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption
)
{
    internal async Task Handle(UpdateDatabaseUserCommand command)
    {
        await new UpdateDatabaseUserCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseUserEntity =
            await dbContext
                .Set<DatabaseUserEntity>()
                .Include(du => du.UserRoles)
                .FirstOrDefaultAsync(d => d.DatabaseUserId == command.DatabaseUserId)
            ?? throw new InvalidOperationException(
                $"DatabaseUser Id '{command.DatabaseUserId}' not found."
            );

        if (
            (
                await dbContext
                    .Set<DatabaseUserEntity>()
                    .Where(u => u.UserName == command.UserName)
                    .AnyAsync()
            )
            && databaseUserEntity.UserName != command.UserName
        )
        {
            throw new InvalidOperationException(
                $"DatabaseUser with name '{command.UserName}' already exists."
            );
        }

        var oldDatabaseUserName = databaseUserEntity.UserName;
        var oldDatabasePassword = encryption.Decrypt(databaseUserEntity.UserPassword);
        var oldDatabaseRoles = databaseUserEntity
            .UserRoles.Select(dr => (DatabaseRole)dr.DatabaseRoleId)
            .ToList();

        if (!string.IsNullOrEmpty(command.UserName))
        {
            databaseUserEntity.UserName = command.UserName;
        }

        if (!string.IsNullOrEmpty(command.UserPassword))
        {
            databaseUserEntity.UserPassword = encryption.Encrypt(command.UserPassword);
        }

        dbContext.Update(databaseUserEntity);
        await dbContext.SaveChangesAsync();

        if (!command.UpdateDatabase)
        {
            return;
        }

        var database = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseServer)
            .FirstOrDefaultAsync(ds => ds.DatabaseId == databaseUserEntity.DatabaseId);

        var commands = new List<string>();
        if (oldDatabaseUserName != command.UserName && !string.IsNullOrEmpty(command.UserName))
        {
            commands.Add(
                $"use {database.DatabaseName}; alter user {oldDatabaseUserName} with name = {command.UserName}"
            );
            commands.Add($"alter login {oldDatabaseUserName} with name = '{command.UserName}'");
        }

        if (
            oldDatabasePassword != command.UserPassword
            && !string.IsNullOrEmpty(command.UserPassword)
        )
        {
            commands.Add(
                $"alter login {command.UserName} with password = '{command.UserPassword}'"
            );
        }

        for (var i = 0; i < commands.Count; i++)
        {
            var commandText = commands[i];
            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();

            if (database.DatabaseServer.IsLinkedServer)
            {
                commandText =
                    $"exec('{commandText.Replace("'", "''")}') at {database.DatabaseServer.DatabaseServerHostName};";
            }

            cmd.CommandText = commandText;
            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
