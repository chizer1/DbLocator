#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUserRoles.DeleteDatabaseUserRole;

internal record DeleteDatabaseUserRoleCommand(
    int DatabaseUserId,
    DatabaseRole UserRole,
    bool affectDatabase = true
);

internal sealed class DeleteDatabaseUserRoleCommandValidator
    : AbstractValidator<DeleteDatabaseUserRoleCommand>
{
    internal DeleteDatabaseUserRoleCommandValidator()
    {
        RuleFor(x => x.DatabaseUserId)
            .GreaterThan(0)
            .WithMessage("Database User Id must be greater than 0.");

        RuleFor(x => x.UserRole)
            .IsInEnum()
            .WithMessage("UserRole must be a valid DatabaseRole enum value.");
    }
}

internal class DeleteDatabaseUserRoleHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        DeleteDatabaseUserRoleCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new DeleteDatabaseUserRoleCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseUserEntity =
            await dbContext
                .Set<DatabaseUserEntity>()
                .FindAsync(new object[] { request.DatabaseUserId }, cancellationToken)
            ?? throw new KeyNotFoundException("Database user not found.");

        var databaseUserRoleEntity = await dbContext
            .Set<DatabaseUserRoleEntity>()
            .FirstOrDefaultAsync(
                ur =>
                    ur.DatabaseUserId == request.DatabaseUserId
                    && ur.DatabaseRoleId == (int)request.UserRole,
                cancellationToken
            );

        if (databaseUserRoleEntity == null)
        {
            return;
        }

        dbContext.Set<DatabaseUserRoleEntity>().Remove(databaseUserRoleEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseUsers");
            await _cache.TryClearConnectionStringFromCache(roles: [request.UserRole]);
        }

        if (!request.affectDatabase)
        {
            return;
        }

        await DropDatabaseUserRole(dbContext, databaseUserRoleEntity, cancellationToken);
    }

    private static async Task DropDatabaseUserRole(
        DbLocatorContext dbContext,
        DatabaseUserRoleEntity databaseUserRoleEntity,
        CancellationToken cancellationToken
    )
    {
        var user =
            await dbContext
                .Set<DatabaseUserEntity>()
                .Include(u => u.Databases)
                .ThenInclude(d => d.Database)
                .ThenInclude(db => db.DatabaseServer)
                .FirstOrDefaultAsync(
                    u => u.DatabaseUserId == databaseUserRoleEntity.DatabaseUserId,
                    cancellationToken
                ) ?? throw new KeyNotFoundException("Database user not found.");

        var roleName =
            Enum.GetName((DatabaseRole)databaseUserRoleEntity.DatabaseRoleId)?.ToLower()
            ?? throw new InvalidOperationException("Invalid role name.");

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

#nullable disable
