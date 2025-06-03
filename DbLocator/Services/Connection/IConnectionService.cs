using DbLocator.Domain;
using Microsoft.Data.SqlClient;

namespace DbLocator.Services.Connection;

internal interface IConnectionService
{
    Task<int> CreateConnection(int tenantId, int databaseId);
    Task DeleteConnection(int connectionId);
    Task<SqlConnection> GetConnection(int tenantId, int databaseTypeId, DatabaseRole[] roles);
    Task<SqlConnection> GetConnection(int connectionId, DatabaseRole[] roles);
    Task<SqlConnection> GetConnection(string tenantCode, int databaseTypeId, DatabaseRole[] roles);
    Task<List<Domain.Connection>> GetConnections();
}
