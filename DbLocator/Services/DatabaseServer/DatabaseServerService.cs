#nullable enable

using DbLocator.Db;
using DbLocator.Features.DatabaseServers.CreateDatabaseServer;
using DbLocator.Features.DatabaseServers.DeleteDatabaseServer;
using DbLocator.Features.DatabaseServers.GetDatabaseServerById;
using DbLocator.Features.DatabaseServers.GetDatabaseServers;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServer;
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
    private readonly UpdateDatabaseServerHandler _updateDatabaseServer =
        new(dbContextFactory, cache);

    public async Task<int> CreateDatabaseServer(
        string databaseServerName,
        string databaseServerHostName,
        string databaseServerIpAddress,
        string databaseServerFullyQualifiedDomainName,
        bool isLinkedServer
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
        return (await _getDatabaseServers.Handle(new GetDatabaseServersQuery())).ToList();
    }

    public async Task<Domain.DatabaseServer> GetDatabaseServer(int databaseServerId)
    {
        return await _getDatabaseServerById.Handle(
            new GetDatabaseServerByIdQuery(databaseServerId)
        );
    }

    public async Task UpdateDatabaseServer(
        int databaseServerId,
        string? databaseServerName,
        string? databaseServerHostName,
        string? databaseServerFullyQualifiedDomainName,
        string? databaseServerIpAddress,
        bool? isLinkedServer
    )
    {
        await _updateDatabaseServer.Handle(
            new UpdateDatabaseServerCommand(
                databaseServerId,
                databaseServerName,
                databaseServerHostName,
                databaseServerFullyQualifiedDomainName,
                databaseServerIpAddress,
                isLinkedServer
            )
        );
    }
}
