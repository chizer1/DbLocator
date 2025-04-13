using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases
{
    internal record DeleteDatabaseCommand(int DatabaseId, bool? DeleteDatabase = false);

    internal sealed class DeleteDatabaseCommandValidator : AbstractValidator<DeleteDatabaseCommand>
    {
        internal DeleteDatabaseCommandValidator()
        {
            RuleFor(x => x.DatabaseId).NotEmpty().WithMessage("Database Id is required.");
        }
    }

    internal class DeleteDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        DbLocatorCache cache
    )
    {
        internal async Task Handle(DeleteDatabaseCommand command)
        {
            await new DeleteDatabaseCommandValidator().ValidateAndThrowAsync(command);

            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseEntity =
                await dbContext.Set<DatabaseEntity>().FindAsync(command.DatabaseId)
                ?? throw new InvalidOperationException("Database not found.");

            if (
                await dbContext
                    .Set<ConnectionEntity>()
                    .AnyAsync(c => c.DatabaseId == command.DatabaseId)
            )
            {
                throw new InvalidOperationException(
                    "Database is being used in Connection table, please remove the connection first if you want to delete this database."
                );
            }

            dbContext.Set<DatabaseEntity>().Remove(databaseEntity);
            await dbContext.SaveChangesAsync();

            if (command.DeleteDatabase == true)
                await DeleteDatabaseAsync(dbContext, databaseEntity);

            cache?.Remove("databases");
            cache?.Remove("connections");

            // TODO: Make this more specific
            cache?.TryClearConnectionStringFromCache(DatabaseTypeId: databaseEntity.DatabaseTypeId);
        }

        private static async Task DeleteDatabaseAsync(
            DbLocatorContext dbContext,
            DatabaseEntity databaseEntity
        )
        {
            var databaseName = Sql.SanitizeSqlIdentifier(databaseEntity.DatabaseName);
            var rawCommand = $"drop database [{databaseName}]";

            var databaseServer = await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == databaseEntity.DatabaseServerId);

            string commandText;
            if (databaseServer?.IsLinkedServer == true)
            {
                var linkedServer = Sql.SanitizeSqlIdentifier(databaseServer.DatabaseServerHostName);
                var escapedCommand = Sql.EscapeForDynamicSql(rawCommand);
                commandText = $"exec('{escapedCommand}') at [{linkedServer}];";
            }
            else
            {
                commandText = rawCommand;
            }

            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = commandText;
            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
