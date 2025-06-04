using DbLocator.Domain;

namespace DbLocator.Services.DatabaseUserRole;

internal interface IDatabaseUserRoleService
{
    Task CreateDatabaseUserRole(int databaseUserId, DatabaseRole userRole, bool updateUser = true);
    Task DeleteDatabaseUserRole(int databaseUserId, DatabaseRole userRole);
    Task DeleteDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool deleteDatabaseUserRole
    );
}
