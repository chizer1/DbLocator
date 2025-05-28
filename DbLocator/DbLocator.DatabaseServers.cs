using DbLocator.Domain;

namespace DbLocator
{
    public partial class Locator
    {
        /// <summary>
        /// Adds a new database server. At least one of the following fields must be provided:
        /// Database Server Host Name, Database Server Fully Qualified Domain Name, or Database Server IP Address.
        /// </summary>
        /// <param name="databaseServerName">
        /// The name of the database server.
        /// </param>
        /// <param name="isLinkedServer">
        /// Indicates whether this server is linked to another server (e.g., the DbLocator database server).
        /// </param>
        /// <param name="databaseServerHostName">
        /// The host name of the database server.
        /// </param>
        /// <param name="databaseServerIpAddress">
        /// The IP address of the database server.
        /// </param>
        /// <param name="databaseServerFullyQualifiedDomainName">
        /// The fully qualified domain name (FQDN) of the database server.
        /// </param>
        /// <returns>
        /// The ID of the newly added database server.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when no valid server identifier (host name, FQDN, or IP address) is provided.
        /// </exception>
        public async Task<int> AddDatabaseServer(
            string databaseServerName,
            bool isLinkedServer,
            string databaseServerHostName = null,
            string databaseServerIpAddress = null,
            string databaseServerFullyQualifiedDomainName = null
        )
        {
            return await _databaseServerService.AddDatabaseServer(
                databaseServerName,
                isLinkedServer,
                databaseServerHostName,
                databaseServerIpAddress,
                databaseServerFullyQualifiedDomainName
            );
        }

        /// <summary>
        /// Retrieves a list of all available database servers.
        /// </summary>
        /// <returns>
        /// A list of <see cref="DatabaseServer"/> representing the available database servers.
        /// </returns>
        public async Task<List<DatabaseServer>> GetDatabaseServers()
        {
            return await _databaseServerService.GetDatabaseServers();
        }

        /// <summary>
        /// Updates an existing database server. At least one of the following fields must be provided:
        /// Database Server Host Name, Database Server Fully Qualified Domain Name, or Database Server IP Address.
        /// </summary>
        /// <param name="databaseServerId">
        /// The ID of the database server to be updated.
        /// </param>
        /// <param name="databaseServerName">
        /// The new name for the database server.
        /// </param>
        /// <param name="databaseServerHostName">
        /// The new host name for the database server.
        /// </param>
        /// <param name="databaseServerFullyQualifiedDomainName">
        /// The new fully qualified domain name (FQDN) for the database server.
        /// </param>
        /// <param name="databaseServerIpAddress">
        /// The new IP address for the database server.
        /// </param>
        /// <param name="isLinkedServer">
        /// Indicates whether this server is linked to another server (e.g., the DbLocator database server).
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when no valid server identifier is provided for the update.
        /// </exception>
        public async Task UpdateDatabaseServer(
            int databaseServerId,
            string databaseServerName,
            string databaseServerHostName,
            string databaseServerFullyQualifiedDomainName,
            string databaseServerIpAddress,
            bool isLinkedServer
        )
        {
            await _databaseServerService.UpdateDatabaseServer(
                databaseServerId,
                databaseServerName,
                databaseServerHostName,
                databaseServerFullyQualifiedDomainName,
                databaseServerIpAddress,
                isLinkedServer
            );
        }

        /// <summary>
        /// Deletes a specified database server.
        /// </summary>
        /// <param name="databaseServerId">
        /// The ID of the database server to be deleted.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to delete a server that has associated databases.
        /// </exception>
        public async Task DeleteDatabaseServer(int databaseServerId)
        {
            await _databaseServerService.DeleteDatabaseServer(databaseServerId);
        }

        /// <summary>
        /// Retrieves a single database server by its ID.
        /// </summary>
        /// <param name="databaseServerId">
        /// The ID of the database server to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="DatabaseServer"/> object representing the database server with the specified ID.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no database server is found with the given ID.
        /// </exception>
        public async Task<DatabaseServer> GetDatabaseServer(int databaseServerId)
        {
            return await _databaseServerService.GetDatabaseServer(databaseServerId);
        }

        /// <summary>
        /// Updates the name of an existing database server.
        /// </summary>
        /// <param name="databaseServerId">
        /// The ID of the database server to be updated.
        /// </param>
        /// <param name="databaseServerName">
        /// The new name for the database server.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server is not found.
        /// </exception>
        public async Task UpdateDatabaseServer(int databaseServerId, string databaseServerName)
        {
            var server = await _databaseServerService.GetDatabaseServer(databaseServerId);
            await _databaseServerService.UpdateDatabaseServer(
                databaseServerId,
                databaseServerName,
                server.HostName,
                server.FullyQualifiedDomainName,
                server.IpAddress,
                server.IsLinkedServer
            );
        }

        /// <summary>
        /// Updates the fully qualified domain name and IP address of an existing database server.
        /// </summary>
        /// <param name="databaseServerId">
        /// The ID of the database server to be updated.
        /// </param>
        /// <param name="databaseServerFullyQualifiedDomainName">
        /// The new fully qualified domain name (FQDN) for the database server.
        /// </param>
        /// <param name="databaseServerIpAddress">
        /// The new IP address for the database server.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server is not found.
        /// </exception>
        public async Task UpdateDatabaseServer(
            int databaseServerId,
            string databaseServerFullyQualifiedDomainName,
            string databaseServerIpAddress
        )
        {
            var server = await _databaseServerService.GetDatabaseServer(databaseServerId);
            await _databaseServerService.UpdateDatabaseServer(
                databaseServerId,
                server.Name,
                server.HostName,
                databaseServerFullyQualifiedDomainName,
                databaseServerIpAddress,
                server.IsLinkedServer
            );
        }

        /// <summary>
        /// Updates the linked server status of an existing database server.
        /// </summary>
        /// <param name="databaseServerId">
        /// The ID of the database server to be updated.
        /// </param>
        /// <param name="isLinkedServer">
        /// Indicates whether this server is linked to another server (e.g., the DbLocator database server).
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server is not found.
        /// </exception>
        public async Task UpdateDatabaseServer(int databaseServerId, bool isLinkedServer)
        {
            var server = await _databaseServerService.GetDatabaseServer(databaseServerId);
            await _databaseServerService.UpdateDatabaseServer(
                databaseServerId,
                server.Name,
                server.HostName,
                server.FullyQualifiedDomainName,
                server.IpAddress,
                isLinkedServer
            );
        }
    }
}
