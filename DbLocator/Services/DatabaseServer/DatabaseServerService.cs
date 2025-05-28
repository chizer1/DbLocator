using DbLocator.Db;
using DbLocator.Features.DatabaseServers.CreateDatabaseServer;
using DbLocator.Features.DatabaseServers.DeleteDatabaseServer;
using DbLocator.Features.DatabaseServers.GetDatabaseServerById;
using DbLocator.Features.DatabaseServers.GetDatabaseServers;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServerName;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServerNetwork;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServerStatus;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseServer;

internal class DatabaseServerService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : IDatabaseServerService
{
    private readonly CreateDatabaseServerHandler _createDatabaseServer =
        new(dbContextFactory, cache);
    private readonly DeleteDatabaseServerHandler _deleteDatabaseServer =
        new(dbContextFactory, cache);
    private readonly GetDatabaseServersHandler _getDatabaseServers = new(dbContextFactory, cache);
    private readonly GetDatabaseServerByIdHandler _getDatabaseServerById =
        new(dbContextFactory, cache);
    private readonly UpdateDatabaseServerNameHandler _updateDatabaseServerName =
        new(dbContextFactory, cache);
    private readonly UpdateDatabaseServerNetworkHandler _updateDatabaseServerNetwork =
        new(dbContextFactory, cache);
    private readonly UpdateDatabaseServerStatusHandler _updateDatabaseServerStatus =
        new(dbContextFactory, cache);

    public async Task<int> AddDatabaseServer(
        string databaseServerName,
        bool isLinkedServer,
        string databaseServerHostName = null,
        string databaseServerIpAddress = null,
        string databaseServerFullyQualifiedDomainName = null
    )
    {
        return await _createDatabaseServer.Handle(
            new CreateDatabaseServerCommand(
                databaseServerName,
                databaseServerHostName,
                databaseServerFullyQualifiedDomainName,
                databaseServerIpAddress,
                isLinkedServer
            )
        );
    }

    public async Task DeleteDatabaseServer(int databaseServerId)
    {
        await _deleteDatabaseServer.Handle(new DeleteDatabaseServerCommand(databaseServerId));
    }

    public async Task<List<Domain.DatabaseServer>> GetDatabaseServers()
    {
        return await _getDatabaseServers.Handle();
    }

    public async Task<Domain.DatabaseServer> GetDatabaseServer(int databaseServerId)
    {
        return await _getDatabaseServerById.Handle(
            new GetDatabaseServerByIdQuery(databaseServerId)
        );
    }

    public async Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName,
        string databaseServerIpAddress,
        bool isLinkedServer
    )
    {
        var server = await GetDatabaseServer(databaseServerId);

        if (databaseServerName != server.Name)
        {
            await _updateDatabaseServerName.Handle(
                new UpdateDatabaseServerNameCommand(databaseServerId, databaseServerName)
            );
        }

        if (
            databaseServerFullyQualifiedDomainName != server.FullyQualifiedDomainName
            || databaseServerIpAddress != server.IpAddress
        )
        {
            await _updateDatabaseServerNetwork.Handle(
                new UpdateDatabaseServerNetworkCommand(
                    databaseServerId,
                    databaseServerFullyQualifiedDomainName,
                    databaseServerIpAddress
                )
            );
        }

        if (isLinkedServer != server.IsLinkedServer)
        {
            await _updateDatabaseServerStatus.Handle(
                new UpdateDatabaseServerStatusCommand(databaseServerId, isLinkedServer)
            );
        }
    }

    public async Task UpdateDatabaseServer(int databaseServerId, string databaseServerName)
    {
        await _updateDatabaseServerName.Handle(
            new UpdateDatabaseServerNameCommand(databaseServerId, databaseServerName)
        );
    }

    public async Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerFullyQualifiedDomainName,
        string databaseServerIpAddress
    )
    {
        await _updateDatabaseServerNetwork.Handle(
            new UpdateDatabaseServerNetworkCommand(
                databaseServerId,
                databaseServerFullyQualifiedDomainName,
                databaseServerIpAddress
            )
        );
    }

    public async Task UpdateDatabaseServer(int databaseServerId, bool isLinkedServer)
    {
        await _updateDatabaseServerStatus.Handle(
            new UpdateDatabaseServerStatusCommand(databaseServerId, isLinkedServer)
        );
    }
}
