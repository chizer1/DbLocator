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
    Encryption encryption,
    DbLocatorCache cache
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
            databaseUserEntity.UserName = command.UserName;

        if (!string.IsNullOrEmpty(command.UserPassword))
            databaseUserEntity.UserPassword = encryption.Encrypt(command.UserPassword);

        dbContext.Update(databaseUserEntity);
        await dbContext.SaveChangesAsync();

        if (!command.UpdateDatabase)
            return;

        var databaseUserDatabases = await dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .Where(dud => dud.DatabaseUserId == databaseUserEntity.DatabaseUserId)
            .ToListAsync();

        if (!databaseUserDatabases.Any())
            throw new InvalidOperationException("No databases found for the DatabaseUser.");

        var commands = new List<string>();

        foreach (var databaseUserDatabase in databaseUserDatabases)
        {
            var database =
                await dbContext
                    .Set<DatabaseEntity>()
                    .Include(d => d.DatabaseServer)
                    .FirstOrDefaultAsync(ds => ds.DatabaseId == databaseUserDatabase.DatabaseId)
                ?? throw new InvalidOperationException("Database not found.");

            if (oldDatabaseUserName != command.UserName && !string.IsNullOrEmpty(command.UserName))
            {
                var sanitizedOldUserName = Sql.SanitizeSqlIdentifier(oldDatabaseUserName);
                var sanitizedNewUserName = Sql.SanitizeSqlIdentifier(command.UserName);
                var sanitizedDbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);

                commands.Add(
                    $"use [{sanitizedDbName}]; alter user [{sanitizedOldUserName}] with name = [{sanitizedNewUserName}]"
                );
                commands.Add(
                    $"alter login [{sanitizedOldUserName}] with name = '{sanitizedNewUserName}'"
                );
            }

            if (
                oldDatabasePassword != command.UserPassword
                && !string.IsNullOrEmpty(command.UserPassword)
            )
            {
                var sanitizedUserName = Sql.SanitizeSqlIdentifier(command.UserName);
                var sanitizedPassword = Sql.EscapeForDynamicSql(command.UserPassword);

                commands.Add(
                    $"alter login [{sanitizedUserName}] with password = '{sanitizedPassword}'"
                );
            }

            for (var i = 0; i < commands.Count; i++)
            {
                var commandText = commands[i];
                using var cmd = dbContext.Database.GetDbConnection().CreateCommand();

                if (database.DatabaseServer.IsLinkedServer)
                {
                    var linkedServerHost = Sql.SanitizeSqlIdentifier(
                        database.DatabaseServer.DatabaseServerHostName
                    );
                    commandText =
                        $"exec(''{Sql.EscapeForDynamicSql(commandText)}') at [{linkedServerHost}];";
                }

                cmd.CommandText = commandText;
                await dbContext.Database.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        cache?.Remove("databaseUsers");

        var roles = databaseUserEntity
            .UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)
            .ToArray();

        cache?.TryClearConnectionStringFromCache(Roles: roles);
    }
}
