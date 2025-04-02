using DbLocator.Domain;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    ///Add database server, need to provide at least one of the following fields: Database Server Host Name, Database Server Fully Qualified Domain Name, Database Server IP Address.
    /// </summary>
    /// <param name="databaseServerName"></param>
    /// <param name="databaseServerIpAddress"></param>
    /// <param name="databaseServerHostName"></param>
    /// <param name="databaseServerFullyQualifiedDomainName"></param>
    /// <param name="isLinkedServer"></param>
    /// <returns>DatabaseServerId</returns>
    public async Task<int> AddDatabaseServer(
        string databaseServerName,
        string databaseServerIpAddress,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName,
        bool isLinkedServer
    )
    {
        return await _databaseServers.AddDatabaseServer(
            databaseServerName,
            isLinkedServer,
            databaseServerIpAddress,
            databaseServerHostName,
            databaseServerFullyQualifiedDomainName
        );
    }

    /// <summary>
    ///Get database servers
    /// </summary>
    /// <returns>List of database servers</returns>
    /// <returns></returns>
    public async Task<List<DatabaseServer>> GetDatabaseServers()
    {
        return await _databaseServers.GetDatabaseServers();
    }

    /// <summary>
    ///Update database server, need to provide at least one of the following fields: Database Server Host Name, Database Server Fully Qualified Domain Name, Database Server IP Address.
    /// </summary>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseServerName"></param>
    /// <param name="databaseServerIpAddress"></param>
    /// <param name="databaseServerHostName"></param>
    /// <param name="databaseServerFullyQualifiedDomainName"></param>
    /// <returns></returns>
    public async Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerIpAddress,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName
    )
    {
        await _databaseServers.UpdateDatabaseServer(
            databaseServerId,
            databaseServerName,
            databaseServerIpAddress,
            databaseServerHostName,
            databaseServerFullyQualifiedDomainName
        );
    }

    /// <summary>
    ///Delete database server
    /// </summary>
    /// <param name="databaseServerId"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseServer(int databaseServerId)
    {
        await _databaseServers.DeleteDatabaseServer(databaseServerId);
    }
}
