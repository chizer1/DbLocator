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
                ?? throw new KeyNotFoundException("Database user not found.");

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
            var userName = Sql.SanitizeSqlIdentifier(databaseUser.UserName);
            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);

            // First try to drop the user from the database
            await Sql.ExecuteSqlCommandAsync(
                dbContext,
                $"use [{dbName}]; if exists (select * from sys.database_principals where name = '{userName}') drop user [{userName}]",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName
            );

            // Then try to drop the login
            await Sql.ExecuteSqlCommandAsync(
                dbContext,
                $"if exists (select * from sys.server_principals where name = '{userName}') drop login [{userName}]",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName
            );
        }
    }
}
