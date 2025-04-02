using DbLocator.Domain;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <param name="UserPassword"></param>
    /// <param name="CreateUser"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        string UserPassword,
        bool CreateUser
    )
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName, UserPassword, CreateUser);
    }

    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <param name="CreateUser"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(int DatabaseId, string UserName, bool CreateUser)
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName, CreateUser);
    }

    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <param name="UserPassword"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(int DatabaseId, string UserName, string UserPassword)
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName, UserPassword);
    }

    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(int DatabaseId, string UserName)
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName);
    }

    /// <summary>
    /// Get database users
    /// </summary>
    /// <returns>List of database users</returns>
    public async Task<List<DatabaseUser>> GetDatabaseUsers()
    {
        return await _databaseUsers.GetDatabaseUsers();
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <param name="DatabaseUserPassword"></param>
    /// <param name="UpdateDatabase"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        string DatabaseUserPassword,
        bool UpdateDatabase
    )
    {
        await _databaseUsers.UpdateDatabaseUser(
            DatabaseUserId,
            DatabaseUserName,
            DatabaseUserPassword,
            UpdateDatabase
        );
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <param name="UpdateDatabase"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        bool UpdateDatabase
    )
    {
        await _databaseUsers.UpdateDatabaseUser(DatabaseUserId, DatabaseUserName, UpdateDatabase);
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <param name="DatabaseUserPassword"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        string DatabaseUserPassword
    )
    {
        await _databaseUsers.UpdateDatabaseUser(
            DatabaseUserId,
            DatabaseUserName,
            DatabaseUserPassword
        );
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(int DatabaseUserId, string DatabaseUserName)
    {
        await _databaseUsers.UpdateDatabaseUser(DatabaseUserId, DatabaseUserName);
    }

    /// <summary>
    /// Delete database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DeleteDatabaseUser"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUser(int DatabaseUserId, bool DeleteDatabaseUser)
    {
        await _databaseUsers.DeleteDatabaseUser(DatabaseUserId, DeleteDatabaseUser);
    }

    /// <summary>
    /// Delete database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUser(int DatabaseUserId)
    {
        await _databaseUsers.DeleteDatabaseUser(DatabaseUserId);
    }
}
