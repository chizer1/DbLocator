using DbLocator.Domain;

namespace DbLocator.Features.DatabaseServers;

internal interface IDatabaseServerRepository
{
    public Task<int> AddDatabaseServer(string databaseServerName, string databaseServerIpAddress);

    public Task<DatabaseServer> GetDatabaseServer(int databaseServerId);

    public Task<List<DatabaseServer>> GetDatabaseServers();

    public Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerIpAddress
    );

    public Task DeleteDatabaseServer(int databaseServerId);
}
