#nullable enable

using DbLocator.Domain;
using Microsoft.Data.SqlClient;

namespace DbLocator.Services.Connection;

internal interface IConnectionService
{
    Task<int> CreateConnection(int tenantId, int databaseId);
    Task DeleteConnection(int connectionId);
    Task<SqlConnection> GetConnection(
        int? tenantId = null,
        int? databaseTypeId = null,
        int? connectionId = null,
        string? tenantCode = null,
        DatabaseRole[]? roles = null
    );
    Task<List<Domain.Connection>> GetConnections();
}
