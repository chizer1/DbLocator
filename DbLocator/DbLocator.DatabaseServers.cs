using DbLocator.Domain;
using FluentValidation;
using Microsoft.Data.SqlClient;

namespace DbLocator;

/// <summary>
/// This partial class contains all database server-related operations for the DbLocator system.
/// It provides comprehensive methods for managing database servers, including:
/// - Creation of new database servers with various configurations
/// - Retrieval of server information
/// - Updates to server settings and network configuration
/// - Deletion of servers
///
/// The database server management system supports:
/// - Multiple server identification methods (hostname, FQDN, IP)
/// - Linked server configurations
/// - Network connectivity management
/// - Server metadata tracking
/// </summary>
public partial class Locator
{
    /// <summary>
    /// Creates a new database server with the specified configuration.
    /// This method establishes a new database server entry in the system, supporting multiple
    /// identification methods for flexible server management. At least one of the following
    /// identifiers must be provided: host name, fully qualified domain name (FQDN), or IP address.
    /// </summary>
    /// <param name="databaseServerName">
    /// The name of the database server. This should be a unique identifier that follows
    /// the system's naming conventions and is easily recognizable in the management interface.
    /// </param>
    /// <param name="isLinkedServer">
    /// Indicates whether this server is linked to another server (e.g., the DbLocator database server).
    /// Linked servers are typically used for distributed database configurations or when the server
    /// is not directly accessible from the DbLocator system.
    /// </param>
    /// <param name="databaseServerHostName">
    /// The host name of the database server. This is typically a short name that identifies
    /// the server within a local network. This parameter is optional if either FQDN or IP
    /// address is provided.
    /// </param>
    /// <param name="databaseServerIpAddress">
    /// The IP address of the database server. This can be either IPv4 or IPv6 format.
    /// This parameter is optional if either host name or FQDN is provided.
    /// </param>
    /// <param name="databaseServerFullyQualifiedDomainName">
    /// The fully qualified domain name (FQDN) of the database server. This is the complete
    /// domain name that uniquely identifies the server on the network. This parameter is
    /// optional if either host name or IP address is provided.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created database server. This ID can be used
    /// to reference the server in future operations and is used internally by the system.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when no valid server identifier (host name, FQDN, or IP address) is provided.
    /// This ensures that the server can be properly identified and accessed.</exception>
    /// <exception cref="ValidationException">Thrown when the server name is invalid or when validation fails.
    /// This includes checks for proper formatting, length, and uniqueness requirements.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or creating the server entry.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
    public async Task<int> CreateDatabaseServer(
        string databaseServerName,
        string databaseServerHostName,
        string databaseServerIpAddress,
        string databaseServerFullyQualifiedDomainName,
        bool isLinkedServer
    )
    {
        return await _databaseServerService.CreateDatabaseServer(
            databaseServerName,
            databaseServerHostName,
            databaseServerIpAddress,
            databaseServerFullyQualifiedDomainName,
            isLinkedServer
        );
    }

    /// <summary>
    /// Retrieves a list of all available database servers in the system.
    /// This method returns comprehensive information about all database servers, including their
    /// configuration, network settings, and associated metadata. The list can be used for
    /// administrative purposes or to audit the system's server configuration.
    /// </summary>
    /// <returns>
    /// A list of <see cref="DatabaseServer"/> objects, each containing detailed information about a server,
    /// including its name, network configuration, and linked server status.
    /// </returns>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the server list.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
    public async Task<List<DatabaseServer>> GetDatabaseServers()
    {
        return await _databaseServerService.GetDatabaseServers();
    }

    /// <summary>
    /// Deletes a specified database server from the system.
    /// This method permanently removes a database server entry from the system. The operation
    /// is irreversible and will fail if the server has any associated databases to prevent
    /// accidental data loss.
    /// </summary>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server to be deleted. This ID must correspond to an
    /// existing database server in the system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the server
    /// has been successfully deleted.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database server is found with the given ID.
    /// This indicates that the server does not exist in the system.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to delete a server that has associated databases.
    /// This prevents accidental deletion of servers that are in use.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the deletion operation fails.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
    public async Task DeleteDatabaseServer(int databaseServerId)
    {
        await _databaseServerService.DeleteDatabaseServer(databaseServerId);
    }

    /// <summary>
    /// Retrieves a single database server by its unique identifier.
    /// This method returns detailed information about a specific database server, including its
    /// configuration, network settings, and associated metadata.
    /// </summary>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server to retrieve. This ID must correspond to an
    /// existing database server in the system.
    /// </param>
    /// <returns>
    /// A <see cref="DatabaseServer"/> object containing detailed information about the server,
    /// including its name, network configuration, and linked server status.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database server is found with the given ID.
    /// This indicates that the server does not exist in the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the server information.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
    public async Task<DatabaseServer> GetDatabaseServer(int databaseServerId)
    {
        return await _databaseServerService.GetDatabaseServer(databaseServerId);
    }

    /// <summary>
    /// Updates the name of an existing database server.
    /// This method allows changing the display name of a database server while preserving all
    /// other configuration settings. The operation is useful for maintaining clear and
    /// consistent server naming conventions.
    /// </summary>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server to be updated. This ID must correspond to an
    /// existing database server in the system.
    /// </param>
    /// <param name="databaseServerName">
    /// The new name for the database server. This should be a unique identifier that follows
    /// the system's naming conventions and is easily recognizable in the management interface.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the server
    /// has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database server is found with the given ID.
    /// This indicates that the server does not exist in the system.</exception>
    /// <exception cref="ValidationException">Thrown when the new server name violates validation rules.
    /// This includes checks for proper formatting, length, and uniqueness requirements.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
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
    /// Updates the network configuration of an existing database server.
    /// This method allows changing the fully qualified domain name (FQDN) and IP address of a
    /// database server while preserving other configuration settings. At least one of the
    /// network identifiers (FQDN or IP address) must be provided.
    /// </summary>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server to be updated. This ID must correspond to an
    /// existing database server in the system.
    /// </param>
    /// <param name="databaseServerFullyQualifiedDomainName">
    /// The new fully qualified domain name (FQDN) for the database server. This is the complete
    /// domain name that uniquely identifies the server on the network. This parameter is
    /// optional if the IP address is provided.
    /// </param>
    /// <param name="databaseServerIpAddress">
    /// The new IP address for the database server. This can be either IPv4 or IPv6 format.
    /// This parameter is optional if the FQDN is provided.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the server
    /// has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database server is found with the given ID.
    /// This indicates that the server does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when both FQDN and IP address are null or empty.
    /// This ensures that the server can be properly identified and accessed.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
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
}
