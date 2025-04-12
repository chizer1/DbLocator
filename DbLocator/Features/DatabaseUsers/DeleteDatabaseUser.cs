using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

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
        IDistributedCache cache
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

            dbContext.Set<DatabaseUserEntity>().Remove(databaseUserEntity);
            await dbContext.SaveChangesAsync();

            cache?.Remove("databaseUsers");

            if (!command.DeleteDatabaseUser)
            {
                return;
            }

            await DropDatabaseUser(dbContext, databaseUserEntity);
        }

        private static async Task DropDatabaseUser(
            DbLocatorContext dbContext,
            DatabaseUserEntity databaseUserEntity
        )
        {
            var database =
                await dbContext
                    .Set<DatabaseEntity>()
                    .Include(d => d.DatabaseServer)
                    .FirstOrDefaultAsync(ds => ds.DatabaseId == databaseUserEntity.DatabaseId)
                ?? throw new InvalidOperationException("Database not found.");

            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
            var userName = Sql.EscapeForDynamicSql(
                Sql.SanitizeSqlIdentifier(databaseUserEntity.UserName)
            );

            var commands = new List<string>
            {
                $"use [{dbName}]; drop user [{userName}]",
                $"drop login [{userName}]"
            };

            foreach (var rawCommand in commands)
            {
                var commandText = rawCommand;

                if (database.DatabaseServer.IsLinkedServer)
                {
                    var linkedServer = Sql.SanitizeSqlIdentifier(
                        database.DatabaseServer.DatabaseServerHostName
                    );
                    commandText =
                        $"exec('{Sql.EscapeForDynamicSql(commandText)}') at [{linkedServer}];";
                }

                using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = commandText;

                await dbContext.Database.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
