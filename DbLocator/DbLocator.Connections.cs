#nullable enable

using DbLocator.Domain;
using FluentValidation;
using Microsoft.Data.SqlClient;

namespace DbLocator;

/// <summary>
/// This partial class contains all connection-related operations for the DbLocator system.
/// It provides comprehensive methods for managing database connections, including:
/// - Creation of new connections between tenants and databases
/// - Retrieval of connections with role-based access control
/// - Deletion of connections with validation
/// - Connection configuration and management
///
/// The connection management system supports:
/// - Role-based access control (RBAC)
/// - Multi-tenant database environments
/// - Different database types
/// - Secure connection handling
/// - Connection validation and enforcement
/// - Connection dependency management
/// </summary>
public partial class Locator
{
    /// <summary>
    /// Retrieves a SQL connection based on the provided connection ID and optional roles.
    /// This method is used to establish a connection to a specific database instance with role-based access control.
    /// The connection is configured based on the stored connection details and the specified roles.
    /// </summary>
    /// <param name="connectionId">
    /// The unique identifier of the connection to retrieve. This ID must correspond to an existing connection in the system.
    /// </param>
    /// <param name="roles">
    /// Optional array of database roles to filter the connection by. If provided, the connection will only be established
    /// if the associated user has at least one of the specified roles. This enables fine-grained access control.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, with a <see cref="SqlConnection"/> result.
    /// The connection is configured with the appropriate credentials and settings for the specified database.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified connection ID does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the connection ID is invalid or when the roles array contains invalid values.</exception>
    /// <exception cref="ValidationException">Thrown when the connection configuration violates validation rules.
    /// This includes checks for connection parameters, role assignments, and security requirements.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no user is found with the specified roles or when there is an error configuring the connection parameters.</exception>
    /// <exception cref="SqlException">Thrown when there is an error establishing the connection to the database server.</exception>
    public async Task<SqlConnection> GetConnection(int connectionId, DatabaseRole[]? roles = null)
    {
        return await _connectionService.GetConnection(connectionId, roles);
    }

    /// <summary>
    /// Retrieves a SQL connection based on the provided tenant ID, database type ID, and optional roles.
    /// This method is used to establish a connection to a database for a specific tenant and database type.
    /// The connection is automatically configured based on the tenant's settings and the specified database type.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant requesting the connection. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type for the connection. This determines the type of database to connect to
    /// (e.g., operational, analytical, reporting).
    /// </param>
    /// <param name="roles">
    /// Optional array of database roles to filter the connection by. If provided, the connection will only be established
    /// if the associated user has at least one of the specified roles. This enables fine-grained access control.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, with a <see cref="SqlConnection"/> result.
    /// The connection is configured with the appropriate credentials and settings for the specified tenant and database type.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified tenant or database type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant ID or database type ID is invalid, or when the roles array contains invalid values.</exception>
    /// <exception cref="ValidationException">Thrown when the connection configuration violates validation rules.
    /// This includes checks for tenant permissions, database type compatibility, and role assignments.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no user is found with the specified roles or when there is an error configuring the connection parameters.</exception>
    /// <exception cref="SqlException">Thrown when there is an error establishing the connection to the database server.</exception>
    public async Task<SqlConnection> GetConnection(
        int tenantId,
        int databaseTypeId,
        DatabaseRole[]? roles = null
    )
    {
        return await _connectionService.GetConnection(tenantId, databaseTypeId, roles);
    }

    /// <summary>
    /// Retrieves a SQL connection based on the provided tenant code, database type ID, and optional roles.
    /// This method is used to establish a connection to a database for a specific tenant (identified by code) and database type.
    /// The connection is automatically configured based on the tenant's settings and the specified database type.
    /// </summary>
    /// <param name="tenantCode">
    /// The unique code identifier of the tenant requesting the connection. This code must correspond to an existing tenant in the system.
    /// The tenant code is typically a more user-friendly identifier than the tenant ID.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type for the connection. This determines the type of database to connect to
    /// (e.g., operational, analytical, reporting).
    /// </param>
    /// <param name="roles">
    /// Optional array of database roles to filter the connection by. If provided, the connection will only be established
    /// if the associated user has at least one of the specified roles. This enables fine-grained access control.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, with a <see cref="SqlConnection"/> result.
    /// The connection is configured with the appropriate credentials and settings for the specified tenant and database type.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified tenant code or database type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant code is invalid, or when the roles array contains invalid values.</exception>
    /// <exception cref="ValidationException">Thrown when the connection configuration violates validation rules.
    /// This includes checks for tenant code format, database type compatibility, and role assignments.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no user is found with the specified roles or when there is an error configuring the connection parameters.</exception>
    /// <exception cref="SqlException">Thrown when there is an error establishing the connection to the database server.</exception>
    public async Task<SqlConnection> GetConnection(
        string tenantCode,
        int databaseTypeId,
        DatabaseRole[]? roles = null
    )
    {
        return await _connectionService.GetConnection(tenantCode, databaseTypeId, roles);
    }

    /// <summary>
    /// Retrieves a list of all connections in the system.
    /// This method returns all configured database connections, including their associated metadata and settings.
    /// The returned list can be used for administrative purposes or to audit the system's connection configuration.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, with a list of <see cref="Connection"/> objects.
    /// Each connection object contains detailed information about the connection configuration, including
    /// tenant information, database details, and connection settings.
    /// </returns>
    /// <exception cref="ValidationException">Thrown when the connection list retrieval violates validation rules.
    /// This includes checks for access permissions and system configuration.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the connection list.</exception>
    public async Task<List<Connection>> GetConnections()
    {
        return await _connectionService.GetConnections();
    }

    /// <summary>
    /// Creates a new connection between the specified tenant and database.
    /// This method establishes a logical connection between a tenant and a database, enabling the tenant to access the database.
    /// The connection is created with default settings and can be configured further if needed.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant for the connection. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <param name="databaseId">
    /// The unique identifier of the database for the connection. This ID must correspond to an existing database in the system.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, with the ID of the newly created connection.
    /// This ID can be used to reference the connection in future operations.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified tenant or database does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant ID or database ID is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the connection creation violates validation rules.
    /// This includes checks for tenant permissions, database availability, and connection limits.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a connection already exists between the specified tenant and database.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or creating the connection record.</exception>
    public async Task<int> CreateConnection(int tenantId, int databaseId)
    {
        return await _connectionService.CreateConnection(tenantId, databaseId);
    }

    /// <summary>
    /// Deletes a connection by its ID.
    /// This method removes the logical connection between a tenant and a database, effectively revoking the tenant's access to the database.
    /// The operation is permanent and cannot be undone.
    /// </summary>
    /// <param name="connectionId">
    /// The unique identifier of the connection to be deleted. This ID must correspond to an existing connection in the system.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task completes when the connection has been successfully deleted.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified connection does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the connection ID is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the connection deletion violates validation rules.
    /// This includes checks for active sessions, dependent operations, and system requirements.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection is currently in use or has active sessions.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or deleting the connection record.</exception>
    public async Task DeleteConnection(int connectionId)
    {
        await _connectionService.DeleteConnection(connectionId);
    }
}
