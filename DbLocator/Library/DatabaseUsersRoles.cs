using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUserRoles;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class DatabaseUserRoles(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    private readonly AddDatabaseUserRole _addDatabaseUserRole = new(dbContextFactory);
    private readonly GetDatabaseUserRoles _getDatabaseUserRoles = new(dbContextFactory, cache);
    private readonly DeleteDatabaseUserRole _deleteDatabaseUserRole = new(dbContextFactory, cache);

    internal async Task AddDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool updateUser
    )
    {
        await _addDatabaseUserRole.Handle(
            new AddDatabaseUserRoleCommand(databaseUserId, userRole, updateUser)
        );
    }

    internal async Task AddDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
    {
        await _addDatabaseUserRole.Handle(
            new AddDatabaseUserRoleCommand(databaseUserId, userRole, false)
        );
    }

    internal async Task<List<DatabaseUserRole>> GetDatabaseUserRoles()
    {
        return await _getDatabaseUserRoles.Handle(new GetDatabaseUserRolesQuery());
    }

    internal async Task DeleteDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
    {
        await _deleteDatabaseUserRole.Handle(
            new DeleteDatabaseUserRoleCommand(databaseUserId, userRole, false)
        );
    }

    internal async Task DeleteDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool deleteDatabaseUserRole
    )
    {
        await _deleteDatabaseUserRole.Handle(
            new DeleteDatabaseUserRoleCommand(databaseUserId, userRole, deleteDatabaseUserRole)
        );
    }
}
