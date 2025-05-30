namespace DbLocator.Services.DatabaseServer;

internal interface IDatabaseServerService
{
    Task<int> CreateDatabaseServer(
        string databaseServerName,
        bool isLinkedServer,
        string databaseServerHostName = null,
        string databaseServerIpCreateress = null,
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
        string databaseServerIpCreateress,
        bool isLinkedServer
    );
    Task UpdateDatabaseServer(int databaseServerId, string databaseServerName);
    Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerFullyQualifiedDomainName,
        string databaseServerIpCreateress
    );
    Task UpdateDatabaseServer(int databaseServerId, bool isLinkedServer);
}
