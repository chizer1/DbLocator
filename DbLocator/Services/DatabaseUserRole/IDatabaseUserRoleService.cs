using DbLocator.Domain;

namespace DbLocator.Services.DatabaseUserRole;

internal interface IDatabaseUserRoleService
{
    Task AddDatabaseUserRole(int databaseUserId, DatabaseRole userRole, bool updateUser);
    Task AddDatabaseUserRole(int databaseUserId, DatabaseRole userRole);
    Task DeleteDatabaseUserRole(int databaseUserId, DatabaseRole userRole);
    Task DeleteDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool deleteDatabaseUserRole
    );
}
