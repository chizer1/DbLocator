using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUserRoles
{
    internal record DeleteDatabaseUserRoleCommand(
        int DatabaseUserId,
        DatabaseRole UserRole,
        bool DeleteDatabaseUserRole
    );

    internal sealed class DeleteDatabaseUserRoleCommandValidator
        : AbstractValidator<DeleteDatabaseUserRoleCommand>
    {
        internal DeleteDatabaseUserRoleCommandValidator()
        {
            RuleFor(x => x.DatabaseUserId).NotEmpty().WithMessage("DatabaseUserId is required.");

            RuleFor(x => x.UserRole)
                .IsInEnum()
                .WithMessage("UserRole must be a valid DatabaseRole enum value.");
        }
    }

    internal class DeleteDatabaseUserRole(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        DbLocatorCache cache
    )
    {
        internal async Task Handle(DeleteDatabaseUserRoleCommand command)
        {
            await new DeleteDatabaseUserRoleCommandValidator().ValidateAndThrowAsync(command);

            await using var dbContext = dbContextFactory.CreateDbContext();

            var databaseUserEntity =
                await dbContext.Set<DatabaseUserEntity>().FindAsync(command.DatabaseUserId)
                ?? throw new InvalidOperationException("DatabaseUser not found.");

            var databaseUserRoleEntity = await dbContext
                .Set<DatabaseUserRoleEntity>()
                .FirstOrDefaultAsync(ur =>
                    ur.DatabaseUserId == command.DatabaseUserId
                    && ur.DatabaseRoleId == (int)command.UserRole
                );

            if (databaseUserRoleEntity == null)
            {
                return;
            }

            dbContext.Set<DatabaseUserRoleEntity>().Remove(databaseUserRoleEntity);
            await dbContext.SaveChangesAsync();

            cache?.TryClearConnectionStringFromCache(Roles: [command.UserRole]);
            if (!command.DeleteDatabaseUserRole)
            {
                return;
            }

            await DropDatabaseUserRole(dbContext, databaseUserRoleEntity);
        }

        private static async Task DropDatabaseUserRole(
            DbLocatorContext dbContext,
            DatabaseUserRoleEntity databaseUserRoleEntity
        )
        {
            var user =
                await dbContext
                    .Set<DatabaseUserEntity>()
                    .Include(u => u.Database)
                    .Include(u => u.Database.DatabaseServer)
                    .FirstOrDefaultAsync(du =>
                        du.DatabaseUserId == databaseUserRoleEntity.DatabaseUserId
                    ) ?? throw new InvalidOperationException("User not found.");

            var roleName = Enum.GetName((DatabaseRole)databaseUserRoleEntity.DatabaseRoleId)
                .ToLower();

            var dbName = Sql.SanitizeSqlIdentifier(user.Database.DatabaseName);
            var userName = Sql.SanitizeSqlIdentifier(user.UserName);

            var commandText =
                $"use [{dbName}]; exec sp_droprolemember 'db_{roleName}', '{userName}'";
            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();

            if (user.Database.DatabaseServer.IsLinkedServer)
            {
                var linkedServerHost = Sql.SanitizeSqlIdentifier(
                    user.Database.DatabaseServer.DatabaseServerHostName
                );
                commandText =
                    $"exec('{Sql.EscapeForDynamicSql(commandText)}') at [{linkedServerHost}];";
            }

            cmd.CommandText = commandText;
            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
