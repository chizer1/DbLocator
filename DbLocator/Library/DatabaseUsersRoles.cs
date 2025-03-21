using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUserRoles;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class DatabaseUserRoles(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    private readonly AddDatabaseUserRole _addDatabaseUserRole = new(dbContextFactory);
    private readonly DeleteDatabaseUserRole _deleteDatabaseUserRole = new(dbContextFactory);

    internal async Task AddDatabaseUserRole(
        int DatabaseUserId,
        DatabaseRole UserRole,
        bool UpdateUser
    )
    {
        await _addDatabaseUserRole.Handle(
            new AddDatabaseUserRoleCommand(DatabaseUserId, UserRole, UpdateUser)
        );
    }

    internal async Task AddDatabaseUserRole(int DatabaseUserId, DatabaseRole UserRole)
    {
        await _addDatabaseUserRole.Handle(
            new AddDatabaseUserRoleCommand(DatabaseUserId, UserRole, false)
        );
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
