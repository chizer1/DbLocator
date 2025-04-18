using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers
{
    internal record DeleteDatabaseUserCommand(int DatabaseUserId, bool DeleteDatabaseUser);

    internal sealed class DeleteDatabaseUserCommandValidator
        : AbstractValidator<DeleteDatabaseUserCommand>
    {
        internal DeleteDatabaseUserCommandValidator()
        {
            RuleFor(x => x.DatabaseUserId).NotEmpty().WithMessage("DatabaseUserId is required.");
        }
    }

    internal class DeleteDatabaseUser(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        DbLocatorCache cache
    )
    {
        internal async Task Handle(DeleteDatabaseUserCommand command)
        {
            await new DeleteDatabaseUserCommandValidator().ValidateAndThrowAsync(command);

            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseUserEntity =
                await dbContext.Set<DatabaseUserEntity>().FindAsync(command.DatabaseUserId)
                ?? throw new InvalidOperationException("DatabaseUser not found.");

            if (
                await dbContext
                    .Set<DatabaseUserRoleEntity>()
                    .AnyAsync(c => c.DatabaseUserId == command.DatabaseUserId)
            )
            {
                throw new InvalidOperationException(
                    "DatabaseUser is being used in DatabaseUserRole table, please remove all associated UserRoles first if you want to delete this database."
                );
            }

            var databaseUserDatabases = await dbContext
                .Set<DatabaseUserDatabaseEntity>()
                .Where(dud => dud.DatabaseUserId == command.DatabaseUserId)
                .ToListAsync();

            if (command.DeleteDatabaseUser)
            {
                var databases = await dbContext
                    .Set<DatabaseUserDatabaseEntity>()
                    .Include(dud => dud.Database)
                    .ThenInclude(db => db.DatabaseServer)
                    .Where(dud => dud.DatabaseUserId == databaseUserEntity.DatabaseUserId)
                    .ToListAsync();

                foreach (var database in databases)
                {
                    await using var scopedDbContext = await dbContextFactory.CreateDbContextAsync();

                    await DropDatabaseUserAsync(
                        scopedDbContext,
                        databaseUserEntity,
                        database.Database
                    );
                }
            }

            dbContext.Set<DatabaseUserDatabaseEntity>().RemoveRange(databaseUserDatabases);
            await dbContext.SaveChangesAsync();

            dbContext.Set<DatabaseUserEntity>().Remove(databaseUserEntity);
            await dbContext.SaveChangesAsync();

            cache?.Remove("databaseUsers");

            var roles = databaseUserEntity
                .UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)
                .ToArray();
            cache?.TryClearConnectionStringFromCache(Roles: roles);
        }

        private static async Task DropDatabaseUserAsync(
            DbLocatorContext dbContext,
            DatabaseUserEntity databaseUser,
            DatabaseEntity database
        )
        {
            var userName = Sql.EscapeForDynamicSql(
                Sql.SanitizeSqlIdentifier(databaseUser.UserName)
            );
            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);

            var commandText = $"USE [{dbName}]; DROP USER [{userName}]";

            if (database.DatabaseServer.IsLinkedServer)
            {
                var linkedServer = Sql.SanitizeSqlIdentifier(
                    database.DatabaseServer.DatabaseServerHostName
                );
                commandText =
                    $"EXEC('{Sql.EscapeForDynamicSql(commandText)}') AT [{linkedServer}];";
            }

            await ExecuteSqlCommandAsync(dbContext, commandText);

            var dropLoginCommand =
                $@"
            IF EXISTS (SELECT * FROM sys.server_principals WHERE name = '{userName}')
            BEGIN
                DROP LOGIN [{userName}]
            END";

            await ExecuteSqlCommandAsync(dbContext, dropLoginCommand);
        }

        private static async Task ExecuteSqlCommandAsync(
            DbLocatorContext dbContext,
            string commandText
        )
        {
            await using var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = commandText;

            await dbContext.Database.OpenConnectionAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}
