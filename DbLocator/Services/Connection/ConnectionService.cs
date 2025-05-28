using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Connections;
using DbLocator.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.Connection;

internal class ConnectionService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache cache
) : IConnectionService
{
    private readonly AddConnection _addConnection = new(dbContextFactory, cache);
    private readonly DeleteConnection _deleteConnection = new(dbContextFactory, cache);
    private readonly GetConnection _getConnection = new(dbContextFactory, encryption, cache);
    private readonly GetConnections _getConnections = new(dbContextFactory, cache);

    public async Task<int> AddConnection(int tenantId, int databaseId)
    {
        return await _addConnection.Handle(new AddConnectionCommand(tenantId, databaseId));
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
        return await _getConnections.Handle(new GetConnectionsQuery());
    }
}
