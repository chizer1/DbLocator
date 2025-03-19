using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers;

internal record UpdateDatabaseUserCommand(
    int DatabaseUserId,
    string DatabaseUserName,
    string DatabaseUserPassword,
    IEnumerable<DatabaseRole> UserRoles,
    bool UpdateDatabase = false
);

internal sealed class UpdateDatabaseUserCommandValidator
    : AbstractValidator<UpdateDatabaseUserCommand>
{
    internal UpdateDatabaseUserCommandValidator()
    {
        RuleFor(x => x.DatabaseUserId).NotNull().WithMessage("DatabaseUser Id is required.");

        RuleFor(x => x.DatabaseUserName)
            .MaximumLength(50)
            .WithMessage("DatabaseUserName cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("DatabaseUserName can only contain letters, numbers, and underscores.");

        RuleFor(x => x.DatabaseUserPassword)
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

        var oldDatabaseUserName = databaseUserEntity.UserName;
        var oldDatabasePassword = encryption.Decrypt(databaseUserEntity.UserPassword);
        var oldDatabaseRoles = databaseUserEntity
            .UserRoles.Select(dr => (DatabaseRole)dr.DatabaseRoleId)
            .ToList();

        if (!string.IsNullOrEmpty(command.DatabaseUserName))
        {
            databaseUserEntity.UserName = command.DatabaseUserName;
        }

        if (!string.IsNullOrEmpty(command.DatabaseUserPassword))
        {
            databaseUserEntity.UserPassword = encryption.Encrypt(command.DatabaseUserPassword);
        }

        if (command.UserRoles != null)
        {
            var databaseRoleEntities = await dbContext
                .Set<DatabaseRoleEntity>()
                .Where(dr => command.UserRoles.Contains((DatabaseRole)dr.DatabaseRoleId))
                .ToListAsync();

            databaseUserEntity.UserRoles = databaseRoleEntities;
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
        if (
            oldDatabaseUserName != command.DatabaseUserName
            && !string.IsNullOrEmpty(command.DatabaseUserName)
        )
        {
            commands.Add(
                $"use {database.DatabaseName}; alter user {oldDatabaseUserName} with name = {command.DatabaseUserName}"
            );
            commands.Add(
                $"alter login {oldDatabaseUserName} with name = '{command.DatabaseUserName}'"
            );
        }

        var dropRoles = oldDatabaseRoles.Except(command.UserRoles).ToList();
        var addRoles = command.UserRoles.Except(oldDatabaseRoles).ToList();

        foreach (var role in dropRoles)
        {
            var roleName = Enum.GetName(role).ToLower();
            commands.Add(
                $"use {database.DatabaseName}; exec sp_droprolemember 'db_{roleName}', '{command.DatabaseUserName}'"
            );
        }

        foreach (var role in addRoles)
        {
            var roleName = Enum.GetName(role).ToLower();
            commands.Add(
                $"use {database.DatabaseName}; exec sp_addrolemember 'db_{roleName}', '{command.DatabaseUserName}'"
            );
        }

        if (
            oldDatabasePassword != command.DatabaseUserPassword
            && !string.IsNullOrEmpty(command.DatabaseUserPassword)
        )
        {
            commands.Add(
                $"alter login {command.DatabaseUserName} with password = '{command.DatabaseUserPassword}'"
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
