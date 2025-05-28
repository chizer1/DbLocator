using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUserRoles;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseUserRole;

internal class DatabaseUserRoleService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : IDatabaseUserRoleService
{
    private readonly AddDatabaseUserRole _addDatabaseUserRole = new(dbContextFactory);
    private readonly DeleteDatabaseUserRole _deleteDatabaseUserRole = new(dbContextFactory, cache);

    public async Task AddDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool updateUser
    )
    {
        await _addDatabaseUserRole.Handle(
            new AddDatabaseUserRoleCommand(databaseUserId, userRole, updateUser)
        );
    }

    public async Task AddDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
    {
        await _addDatabaseUserRole.Handle(
            new AddDatabaseUserRoleCommand(databaseUserId, userRole, true)
        );
    }

    public async Task DeleteDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
    {
        await _deleteDatabaseUserRole.Handle(
            new DeleteDatabaseUserRoleCommand(databaseUserId, userRole, true)
        );
    }

    public async Task DeleteDatabaseUserRole(
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
