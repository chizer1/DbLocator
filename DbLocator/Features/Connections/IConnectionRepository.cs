using System.Data.SqlClient;
using DbLocator.Domain;

namespace DbLocator.Features.Connections;

internal interface IConnectionRepository
{
    public Task<int> AddConnection(int tenantId, int databaseId);
    public Task<SqlConnection> GetSqlConnection(int tenantId, int databaseTypeId);
    public Task DeleteConnection(int connectionId);
    public Task<List<Connection>> GetConnections();
}
