#nullable enable

namespace DbLocator.Services.DatabaseServer;

internal interface IDatabaseServerService
{
    Task<int> CreateDatabaseServer(
        string databaseServerName,
        string databaseServerHostName,
        string databaseServerIpAddress,
        string databaseServerFullyQualifiedDomainName,
        bool isLinkedServer
    );
    Task DeleteDatabaseServer(int databaseServerId);
    Task<List<Domain.DatabaseServer>> GetDatabaseServers();
    Task<Domain.DatabaseServer> GetDatabaseServer(int databaseServerId);
    Task UpdateDatabaseServer(
        int databaseServerId,
        string? databaseServerName,
        string? databaseServerHostName,
        string? databaseServerFullyQualifiedDomainName,
        string? databaseServerIpAddress,
        bool? isLinkedServer
    );
}
