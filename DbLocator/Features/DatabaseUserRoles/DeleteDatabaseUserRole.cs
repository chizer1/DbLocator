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
        bool AffectDatabase
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
                ?? throw new KeyNotFoundException("Database user not found.");

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
            if (!command.AffectDatabase)
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
                    .Include(u => u.Databases)
                    .ThenInclude(d => d.Database)
                    .ThenInclude(db => db.DatabaseServer)
                    .FirstOrDefaultAsync(u =>
                        u.DatabaseUserId == databaseUserRoleEntity.DatabaseUserId
                    ) ?? throw new KeyNotFoundException("Database user not found.");

            var roleName = Enum.GetName((DatabaseRole)databaseUserRoleEntity.DatabaseRoleId)
                .ToLower();

            var databases = user.Databases.Select(d => d.Database).ToList();
            foreach (var database in databases)
            {
                var userName = Sql.SanitizeSqlIdentifier(user.UserName);

                await Sql.ExecuteSqlCommandAsync(
                    dbContext,
                    $"use [{database.DatabaseName}]; exec sp_droprolemember 'db_{roleName}', '{userName}'",
                    database.DatabaseServer.IsLinkedServer,
                    database.DatabaseServer.DatabaseServerHostName
                );
            }
        }
    }
}
