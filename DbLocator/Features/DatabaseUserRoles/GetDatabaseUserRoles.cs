using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUserRoles;

/// <summary>
/// Query to get all database user roles.
/// </summary>
public class GetDatabaseUserRolesQuery
{
    /// <summary>
    /// Gets all database user roles.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="cache">The cache service.</param>
    /// <returns>A list of database user roles.</returns>
    internal static async Task<List<DatabaseUserRole>> GetDatabaseUserRoles(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        IDbLocatorCache cache
    )
    {
        var cacheKey = "databaseUserRoles";
        var cachedData = await cache.GetCachedData<List<DatabaseUserRole>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        using var context = await dbContextFactory.CreateDbContextAsync();
        var userRoles = await context
            .DatabaseUserRoles.AsNoTracking()
            .Include(ur => ur.User)
            .AsNoTracking()
            .Select(ur => new DatabaseUserRole(
                ur.DatabaseUserRoleId,
                ur.User.UserName,
                (DatabaseRole)ur.DatabaseRoleId
            ))
            .ToListAsync();

        await cache.CacheData(cacheKey, userRoles);
        return userRoles;
    }
}

/// <summary>
/// Validator for the GetDatabaseUserRolesQuery.
/// </summary>
public sealed class GetDatabaseUserRolesQueryValidator
    : AbstractValidator<GetDatabaseUserRolesQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDatabaseUserRolesQueryValidator"/> class.
    /// </summary>
    public GetDatabaseUserRolesQueryValidator() { }
}

/// <summary>
/// Handler for getting database user roles.
/// </summary>
internal class GetDatabaseUserRoles(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    /// <summary>
    /// Handles the request to get database user roles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>A list of database user roles.</returns>
    public async Task<List<DatabaseUserRole>> Handle(GetDatabaseUserRolesQuery query)
    {
        await new GetDatabaseUserRolesQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databaseUserRoles";

        var cachedData = await cache?.GetCachedData<List<DatabaseUserRole>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseUserRoles = await GetDatabaseUserRolesFromDatabase(dbContextFactory);
        await cache?.CacheData(cacheKey, databaseUserRoles);

        return databaseUserRoles;
    }

    private static async Task<List<DatabaseUserRole>> GetDatabaseUserRolesFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseUserRoleEntities = await dbContext
            .Set<DatabaseUserRoleEntity>()
            .Include(r => r.User)
            .Include(r => r.Role)
            .ToListAsync();

        var databaseUserRoles = databaseUserRoleEntities
            .Select(role => new DatabaseUserRole(
                role.DatabaseUserRoleId,
                role.User.UserName,
                (DatabaseRole)role.DatabaseRoleId
            ))
            .ToList();

        return databaseUserRoles;
    }
}
