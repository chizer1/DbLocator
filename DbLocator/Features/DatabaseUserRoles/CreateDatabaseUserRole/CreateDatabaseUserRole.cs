#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUserRoles.CreateDatabaseUserRole;

internal record CreateDatabaseUserRoleCommand(
    int DatabaseUserId,
    DatabaseRole UserRole,
    bool AffectDatabase
);

internal sealed class CreateDatabaseUserRoleCommandValidator
    : AbstractValidator<CreateDatabaseUserRoleCommand>
{
    internal CreateDatabaseUserRoleCommandValidator()
    {
        RuleFor(x => x.DatabaseUserId)
            .GreaterThan(0)
            .WithMessage("Database User Id must be greater than 0.");
    }
}

internal class CreateDatabaseUserRoleHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<int> Handle(
        CreateDatabaseUserRoleCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new CreateDatabaseUserRoleCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var user =
            await dbContext
                .Set<DatabaseUserEntity>()
                .Include(u => u.UserRoles)
                .Where(u => u.DatabaseUserId == request.DatabaseUserId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Database User Id '{request.DatabaseUserId}' not found."
            );

        var existingRole = user.UserRoles.FirstOrDefault(ur =>
            ur.DatabaseRoleId == (int)request.UserRole
        );

        if (existingRole != null)
            throw new InvalidOperationException(
                $"User '{user.UserName}' already has role '{request.UserRole}'."
            );

        var databaseUserRole = new DatabaseUserRoleEntity()
        {
            DatabaseUserId = request.DatabaseUserId,
            DatabaseRoleId = (int)request.UserRole
        };

        await dbContext.Set<DatabaseUserRoleEntity>().AddAsync(databaseUserRole, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.AffectDatabase)
            await CreateDatabaseUserRole(dbContext, user, request, cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseUsers");
            await _cache.TryClearConnectionStringFromCache(roles: [request.UserRole]);
        }

        return databaseUserRole.DatabaseUserRoleId;
    }

    private static async Task CreateDatabaseUserRole(
        DbLocatorContext dbContext,
        DatabaseUserEntity user,
        CreateDatabaseUserRoleCommand request,
        CancellationToken cancellationToken
    )
    {
        var databases = await dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .Include(dud => dud.Database)
            .Include(dud => dud.Database.DatabaseServer)
            .Where(dud => dud.DatabaseUserId == user.DatabaseUserId)
            .Select(dud => dud.Database)
            .ToListAsync(cancellationToken);

        foreach (var database in databases)
        {
            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
            var userName = Sql.SanitizeSqlIdentifier(user.UserName);
            var roleName = Sql.SanitizeSqlIdentifier($"db_{request.UserRole.ToString().ToLower()}");

            await Sql.ExecuteSqlCommandAsync(
                dbContext,
                $"use [{dbName}]; exec sp_addrolemember '{roleName}', '{userName}';",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName
            );
        }
    }
}
