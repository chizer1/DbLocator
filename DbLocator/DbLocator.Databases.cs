using DbLocator.Domain;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseStatus"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus
        );
    }

    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseStatus"></param>
    /// <param name="createDatabase"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool createDatabase
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus,
            createDatabase
        );
    }

    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            Status.Active
        );
    }

    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="createDatabase"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool createDatabase
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            createDatabase
        );
    }

    /// <summary>
    ///Get databases
    /// </summary>
    /// <returns>List of Databases</returns>
    public async Task<List<Database>> GetDatabases()
    {
        return await _databases.GetDatabases();
    }

    /// <summary>
    ///Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseStatus"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(
        int databaseId,
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        await _databases.UpdateDatabase(
            databaseId,
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus
        );
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseServerId"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, int databaseServerId)
    {
        await _databases.UpdateDatabase(databaseId, databaseServerId);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseTypeId"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, byte databaseTypeId)
    {
        await _databases.UpdateDatabase(databaseId, databaseTypeId);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, string databaseName)
    {
        await _databases.UpdateDatabase(databaseId, databaseName);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseStatus"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, Status databaseStatus)
    {
        await _databases.UpdateDatabase(databaseId, databaseStatus);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="useTrustedConnection"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, bool useTrustedConnection)
    {
        await _databases.UpdateDatabase(databaseId, useTrustedConnection);
    }

    /// <summary>
    ///Delete database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    public async Task DeleteDatabase(int databaseId)
    {
        await _databases.DeleteDatabase(databaseId);
    }

    /// <summary>
    ///Delete database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="deleteDatabase"></param>
    /// <returns></returns>
    public async Task DeleteDatabase(int databaseId, bool deleteDatabase)
    {
        await _databases.DeleteDatabase(databaseId, deleteDatabase);
    }
}
