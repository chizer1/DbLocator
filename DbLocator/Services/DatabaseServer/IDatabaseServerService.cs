namespace DbLocator.Services.DatabaseServer;

internal interface IDatabaseServerService
{
    Task<int> CreateDatabaseServer(
        string databaseServerName,
        bool isLinkedServer,
        string databaseServerHostName = null,
        string databaseServerIpAddress = null,
        string databaseServerFullyQualifiedDomainName = null
    );
    Task DeleteDatabaseServer(int databaseServerId);
    Task<List<Domain.DatabaseServer>> GetDatabaseServers();
    Task<Domain.DatabaseServer> GetDatabaseServer(int databaseServerId);
    Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName,
        string databaseServerIpAddress,
        bool isLinkedServer
    );
}
