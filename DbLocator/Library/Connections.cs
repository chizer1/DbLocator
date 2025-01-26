using System.Data.SqlClient;
using DbLocator.Domain;
using DbLocator.Features.Connections;
using DbLocator.Features.Connections.AddConnection;
using DbLocator.Features.Connections.DeleteConnection;
using DbLocator.Features.Connections.GetConnection;
using DbLocator.Features.Connections.GetConnections;
using DbLocator.Features.Tenants;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class Connections
{
    private readonly AddConnection _addConnection;
    private readonly GetConnection _getConnection;
    private readonly GetConnections _getConnections;
    private readonly DeleteConnection _deleteConnection;

    public Connections(DbContext dbContext)
    {
        IConnectionRepository connectionRepository = new ConnectionRepository(dbContext);
        ITenantRepository tenantRepository = new TenantRepository(dbContext);

        _addConnection = new AddConnection(connectionRepository);
        _getConnection = new GetConnection(connectionRepository, tenantRepository);
        _getConnections = new GetConnections(connectionRepository);
        _deleteConnection = new DeleteConnection(connectionRepository);
    }

    public async Task<int> AddConnection(int tenantId, int databaseId)
    {
        return await _addConnection.Handle(new AddConnectionCommand(tenantId, databaseId));
    }

    public async Task<SqlConnection> GetConnection(int tenantId, int databaseTypeId)
    {
        return await _getConnection.Handle(new GetConnectionQuery(tenantId, databaseTypeId));
    }

    public async Task<List<Connection>> GetConnections()
    {
        return await _getConnections.Execute();
    }

    public async Task DeleteConnection(int connectionId)
    {
        await _deleteConnection.Handle(new DeleteConnectionCommand(connectionId));
    }
}
