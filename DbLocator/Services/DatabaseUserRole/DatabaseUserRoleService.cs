using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUserRoles.CreateDatabaseUserRole;
using DbLocator.Features.DatabaseUserRoles.DeleteDatabaseUserRole;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseUserRole;

internal class DatabaseUserRoleService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : IDatabaseUserRoleService
{
    private readonly CreateDatabaseUserRoleHandler _createDatabaseUserRole =
        new(dbContextFactory, cache);
    private readonly DeleteDatabaseUserRoleHandler _deleteDatabaseUserRole =
        new(dbContextFactory, cache);

    public async Task CreateDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool updateUser
    )
    {
        await _createDatabaseUserRole.Handle(
            new CreateDatabaseUserRoleCommand(databaseUserId, userRole, updateUser)
        );
    }

    public async Task CreateDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
    {
        await _createDatabaseUserRole.Handle(
            new CreateDatabaseUserRoleCommand(databaseUserId, userRole, true)
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
