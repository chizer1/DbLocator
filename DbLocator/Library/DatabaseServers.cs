using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseServers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Library;

internal class DatabaseServers(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    private readonly AddDatabaseServer _addDatabaseServer = new(dbContextFactory, cache);
    private readonly DeleteDatabaseServer _deleteDatabaseServer = new(dbContextFactory, cache);
    private readonly GetDatabaseServers _getDatabaseServers = new(dbContextFactory, cache);
    private readonly UpdateDatabaseServer _updateDatabaseServer = new(dbContextFactory, cache);

    internal async Task<int> AddDatabaseServer(
        string databaseServerName,
        bool isLinkedServer,
        string databaseServerHostName = null,
        string databaseServerIpAddress = null,
        string databaseServerFullyQualifiedDomainName = null
    )
    {
        return await _addDatabaseServer.Handle(
            new AddDatabaseServerCommand(
                databaseServerName,
                databaseServerHostName,
                databaseServerIpAddress,
                databaseServerFullyQualifiedDomainName,
                isLinkedServer
            )
        );
    }

    internal async Task DeleteDatabaseServer(int databaseServerId)
    {
        await _deleteDatabaseServer.Handle(new DeleteDatabaseServerCommand(databaseServerId));
    }

    internal async Task<List<DatabaseServer>> GetDatabaseServers()
    {
        return await _getDatabaseServers.Handle(new GetDatabaseServersQuery());
    }

    internal async Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerIpAddress = null,
        string databaseServerHostName = null,
        string databaseServerFullyQualifiedDomainName = null
    )
    {
        await _updateDatabaseServer.Handle(
            new UpdateDatabaseServerCommand(
                databaseServerId,
                databaseServerName,
                databaseServerIpAddress,
                databaseServerHostName,
                databaseServerFullyQualifiedDomainName
            )
        );
    }
}
