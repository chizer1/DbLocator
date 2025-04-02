using DbLocator.Domain;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    /// Add database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <param name="UpdateUser"></param>
    /// <returns></returns>
    public async Task AddDatabaseUserRole(
        int DatabaseUserId,
        DatabaseRole UserRole,
        bool UpdateUser
    )
    {
        await _databaseUserRoles.AddDatabaseUserRole(DatabaseUserId, UserRole, UpdateUser);
    }

    /// <summary>
    /// Add database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <returns></returns>
    public async Task AddDatabaseUserRole(int DatabaseUserId, DatabaseRole UserRole)
    {
        await _databaseUserRoles.AddDatabaseUserRole(DatabaseUserId, UserRole);
    }

    /// <summary>
    /// Delete database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <param name="DeleteDatabaseUserRole"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUserRole(
        int DatabaseUserId,
        DatabaseRole UserRole,
        bool DeleteDatabaseUserRole
    )
    {
        await _databaseUserRoles.DeleteDatabaseUserRole(
            DatabaseUserId,
            UserRole,
            DeleteDatabaseUserRole
        );
    }

    /// <summary>
    /// Delete database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUserRole(int DatabaseUserId, DatabaseRole UserRole)
    {
        await _databaseUserRoles.DeleteDatabaseUserRole(DatabaseUserId, UserRole);
    }
}
