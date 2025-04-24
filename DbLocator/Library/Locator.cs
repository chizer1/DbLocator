using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUserRoles;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

/// <summary>
/// Main library class for database operations.
/// </summary>
public class Locator
{
    private readonly DatabaseUserRoles _databaseUserRoles;

    public Locator(IDbContextFactory<DbLocatorContext> dbContextFactory, IDbLocatorCache cache)
    {
        _databaseUserRoles = new DatabaseUserRoles(dbContextFactory, cache);
    }

    /// <summary>
    /// Gets all database user roles.
    /// </summary>
    /// <returns>A list of database user roles.</returns>
    public async Task<List<DatabaseUserRole>> GetDatabaseUserRoles()
    {
        return await _databaseUserRoles.GetDatabaseUserRoles();
    }

    // ... existing code ...
}
