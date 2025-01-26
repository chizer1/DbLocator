using DbLocator.Domain;
using DbLocator.Features.DatabaseServers;
using DbLocator.Features.DatabaseServers.AddDatabaseServer;
using DbLocator.Features.DatabaseServers.DeleteDatabaseServer;
using DbLocator.Features.DatabaseServers.GetDatabaseServers;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServer;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class DatabaseServers
{
    private readonly AddDatabaseServer _addDatabaseServer;
    private readonly GetDatabaseServers _getDatabaseServers;
    private readonly UpdateDatabaseServer _updateDatabaseServer;
    private readonly DeleteDatabaseServer _deleteDatabaseServer;

    public DatabaseServers(string dbLocatorConnectionString)
    {
        var factory = DbContextFactory.CreateDbContextFactory(dbLocatorConnectionString);

        IDatabaseServerRepository databaseServerRepository = new DatabaseServerRepository(factory);

        _addDatabaseServer = new AddDatabaseServer(databaseServerRepository);
        _getDatabaseServers = new GetDatabaseServers(databaseServerRepository);
        _updateDatabaseServer = new UpdateDatabaseServer(databaseServerRepository);
        _deleteDatabaseServer = new DeleteDatabaseServer(databaseServerRepository);
    }

    public async Task<int> AddDatabaseServer(
        string databaseServerName,
        string databaseServerIpAddress
    )
    {
        return await _addDatabaseServer.Handle(
            new AddDatabaseServerCommand(databaseServerName, databaseServerIpAddress)
        );
    }

    public async Task<List<DatabaseServer>> GetDatabaseServers()
    {
        return await _getDatabaseServers.Handle(new GetDatabaseServersQuery());
    }

    public async Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerIpAddress
    )
    {
        await _updateDatabaseServer.Handle(
            new UpdateDatabaseServerCommand(
                databaseServerId,
                databaseServerName,
                databaseServerIpAddress
            )
        );
    }

    public async Task DeleteDatabaseServer(int databaseServerId)
    {
        await _deleteDatabaseServer.Handle(new DeleteDatabaseServerCommand(databaseServerId));
    }
}
