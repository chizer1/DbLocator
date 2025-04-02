using DbLocator.Domain;
using Microsoft.Data.SqlClient;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    /// Get SQL connection
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="roles"></param>
    /// <returns>SqlConnection</returns>
    public async Task<SqlConnection> GetConnection(int connectionId, DatabaseRole[] roles = null)
    {
        return await _connections.GetConnection(connectionId, roles);
    }

    /// <summary>
    /// Get SQL connection
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="roles"></param>
    /// <returns>SqlConnection</returns>
    /// <returns></returns>
    public async Task<SqlConnection> GetConnection(
        int tenantId,
        int databaseTypeId,
        DatabaseRole[] roles = null
    )
    {
        return await _connections.GetConnection(tenantId, databaseTypeId, roles);
    }

    /// <summary>
    /// Get SQL connection
    /// </summary>
    /// <param name="tenantCode"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="roles"></param>
    /// <returns>SqlConnection</returns>
    public async Task<SqlConnection> GetConnection(
        string tenantCode,
        int databaseTypeId,
        DatabaseRole[] roles = null
    )
    {
        return await _connections.GetConnection(tenantCode, databaseTypeId, roles);
    }

    /// <summary>
    /// Get connections
    /// </summary>
    /// <returns>List of connections</returns>
    /// <returns></returns>
    public async Task<List<Connection>> GetConnections()
    {
        return await _connections.GetConnections();
    }

    /// <summary>
    ///Add connection
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="databaseId"></param>
    /// <returns>ConnectionId</returns>
    public async Task<int> AddConnection(int tenantId, int databaseId)
    {
        return await _connections.AddConnection(tenantId, databaseId);
    }

    /// <summary>
    ///Delete connection
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public async Task DeleteConnection(int connectionId)
    {
        await _connections.DeleteConnection(connectionId);
    }
}
