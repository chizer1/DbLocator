using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Connections;
using DbLocator.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class Connections(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache cache
)
{
    private readonly AddConnection _addConnection = new(dbContextFactory, cache);
    private readonly DeleteConnection _deleteConnection = new(dbContextFactory, cache);
    private readonly GetConnection _getConnection = new(dbContextFactory, encryption, cache);
    private readonly GetConnections _getConnections = new(dbContextFactory, cache);

    internal async Task<int> AddConnection(int tenantId, int databaseId)
    {
        return await _addConnection.Handle(new AddConnectionCommand(tenantId, databaseId));
    }

    internal async Task DeleteConnection(int connectionId)
    {
        await _deleteConnection.Handle(new DeleteConnectionCommand(connectionId));
    }

    internal async Task<SqlConnection> GetConnection(
        int tenantId,
        int databaseTypeId,
        DatabaseRole[] roles
    )
    {
        return await _getConnection.Handle(
            new GetConnectionQuery(tenantId, databaseTypeId, null, null, roles)
        );
    }

    internal async Task<SqlConnection> GetConnection(int connectionId, DatabaseRole[] roles)
    {
        return await _getConnection.Handle(
            new GetConnectionQuery(null, null, connectionId, null, roles)
        );
    }

    internal async Task<SqlConnection> GetConnection(
        string tenantCode,
        int databaseTypeId,
        DatabaseRole[] roles
    )
    {
        return await _getConnection.Handle(
            new GetConnectionQuery(null, databaseTypeId, null, tenantCode, roles)
        );
    }

    internal async Task<List<Connection>> GetConnections()
    {
        return await _getConnections.Handle(new GetConnectionsQuery());
    }
}
