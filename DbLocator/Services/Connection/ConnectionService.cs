#nullable enable

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

    public async Task<int> CreateConnection(int tenantId, int databaseId)
    {
        return await _createConnection.Handle(new CreateConnectionCommand(tenantId, databaseId));
    }

    public async Task DeleteConnection(int connectionId)
    {
        await _deleteConnection.Handle(new DeleteConnectionCommand(connectionId));
    }

    public async Task<SqlConnection> GetConnection(
        int? tenantId = null,
        int? databaseTypeId = null,
        int? connectionId = null,
        string? tenantCode = null,
        DatabaseRole[]? roles = null
    )
    {
        return await _getConnection.Handle(
            new GetConnectionQuery(tenantId, databaseTypeId, connectionId, tenantCode, roles)
        );
    }

    public async Task<List<Domain.Connection>> GetConnections()
    {
        var connections = await _getConnections.Handle(new GetConnectionsQuery());
        return connections.ToList();
    }
}
