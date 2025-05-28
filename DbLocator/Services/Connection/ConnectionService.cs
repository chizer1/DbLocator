using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Connections.CreateConnection;
using DbLocator.Features.Connections.DeleteConnection;
using DbLocator.Features.Connections.GetConnection;
using DbLocator.Features.Connections.GetConnections;
using DbLocator.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.Connection;

internal class ConnectionService(
    IDbContextFactory<DbLocatorContext> contextFactory,
    DbLocatorCache cache,
    Encryption encryption
) : IConnectionService
{
    private readonly CreateConnectionHandler _createConnection = new(contextFactory, cache);
    private readonly DeleteConnectionHandler _deleteConnection = new(contextFactory, cache);
    private readonly GetConnectionHandler _getConnection = new(contextFactory, encryption, cache);
    private readonly GetConnectionsHandler _getConnections = new(contextFactory, cache);

    public async Task<int> AddConnection(int tenantId, int databaseId)
    {
        return await _createConnection.Handle(new CreateConnectionCommand(tenantId, databaseId));
    }

    public async Task DeleteConnection(int connectionId)
    {
        await _deleteConnection.Handle(new DeleteConnectionCommand(connectionId));
    }

    public async Task<SqlConnection> GetConnection(
        int tenantId,
        int databaseTypeId,
        DatabaseRole[] roles
    )
    {
        return await _getConnection.Handle(
            new GetConnectionQuery(tenantId, databaseTypeId, null, null, roles)
        );
    }

    public async Task<SqlConnection> GetConnection(int connectionId, DatabaseRole[] roles)
    {
        return await _getConnection.Handle(
            new GetConnectionQuery(null, null, connectionId, null, roles)
        );
    }

    public async Task<SqlConnection> GetConnection(
        string tenantCode,
        int databaseTypeId,
        DatabaseRole[] roles
    )
    {
        return await _getConnection.Handle(
            new GetConnectionQuery(null, databaseTypeId, null, tenantCode, roles)
        );
    }

    public async Task<List<Domain.Connection>> GetConnections()
    {
        var connections = await _getConnections.Handle(new GetConnectionsQuery());
        return connections.ToList();
    }
}
