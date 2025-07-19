using DbLocator.Domain;
using FluentValidation;
using Microsoft.Data.SqlClient;

namespace DbLocator;

/// <summary>
/// This partial class contains all database-related operations for the DbLocator system.
/// It provides comprehensive methods for managing databases, including:
/// - Creation of new databases with various configurations
/// - Retrieval of database information
/// - Updates to database settings and properties
/// - Deletion of databases with optional physical removal
/// </summary>
public partial class Locator
{
    /// <summary>
    /// Creates a new database with the specified configuration.
    /// This method establishes a new database instance on the specified server with the given type and status.
    /// The database is created with default settings and can be configured further if needed.
    /// </summary>
    /// <param name="databaseName">
    /// The name of the database to be created. Must be a valid SQL Server database name.
    /// The name should follow SQL Server naming conventions and should not contain special characters.
    /// </param>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server where the database will be created.
    /// This ID must correspond to an existing database server in the system.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the logical database type (e.g., Operational, Analytical).
    /// This determines the category and intended use of the database.
    /// </param>
    /// <param name="databaseStatus">
    /// The initial status of the database (e.g., Active, Inactive).
    /// This status can be updated later using the UpdateDatabase method.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created database.
    /// This ID can be used to reference the database in future operations.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database server or database type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database name is null, empty, or invalid, or when attempting to create a database with invalid parameters.</exception>
    /// <exception cref="ValidationException">Thrown when the database name violates business validation rules or when the database configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database with the same name already exists on the server.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database server or creating the database.</exception>
    public async Task<int> CreateDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        return await _databaseService.CreateDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus,
            true,
            false
        );
    }

    /// <summary>
    /// Creates a new database with the specified configuration and optional DDL operation control.
    /// This method provides more control over the database creation process by allowing the caller to specify
    /// whether to perform actual DDL operations on the database server.
    /// </summary>
    /// <param name="databaseName">
    /// The name of the database to be created. Must be a valid SQL Server database name.
    /// The name should follow SQL Server naming conventions and should not contain special characters.
    /// </param>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server where the database will be created.
    /// This ID must correspond to an existing database server in the system.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type. This determines the category and intended use of the database.
    /// </param>
    /// <param name="databaseStatus">
    /// The initial status of the database (e.g., Active, Inactive).
    /// This status can be updated later using the UpdateDatabase method.
    /// </param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server.
    /// If set to false, the database will only be registered in the system without being physically created.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created database.
    /// This ID can be used to reference the database in future operations.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database server or database type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database name is null, empty, or invalid, or when attempting to create a database with invalid parameters.</exception>
    /// <exception cref="ValidationException">Thrown when the database name violates business validation rules or when the database configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database with the same name already exists on the server.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database server or creating the database.</exception>
    public async Task<int> CreateDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true
    )
    {
        return await _databaseService.CreateDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus,
            affectDatabase,
            false
        );
    }

    /// <summary>
    /// Creates a new database with the default status of Active.
    /// This is a convenience method that creates a database with the most common configuration.
    /// The database is created with Active status and can be configured further if needed.
    /// </summary>
    /// <param name="databaseName">
    /// The name of the database to be created. Must be a valid SQL Server database name.
    /// The name should follow SQL Server naming conventions and should not contain special characters.
    /// </param>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server where the database will be created.
    /// This ID must correspond to an existing database server in the system.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type. This determines the category and intended use of the database.
    /// </param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server.
    /// If set to false, the database will only be registered in the system without being physically created.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created database.
    /// This ID can be used to reference the database in future operations.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database server or database type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database name is null, empty, or invalid, or when attempting to create a database with invalid parameters.</exception>
    /// <exception cref="ValidationException">Thrown when the database name violates business validation rules or when the database configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database with the same name already exists on the server.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database server or creating the database.</exception>
    public async Task<int> CreateDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool affectDatabase = true
    )
    {
        return await _databaseService.CreateDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            Status.Active,
            affectDatabase,
            false
        );
    }

    /// <summary>
    /// Creates a new database with specified trusted connection settings.
    /// This method allows for the creation of a database with Windows authentication support.
    /// The database is created with Active status by default and can be configured further if needed.
    /// </summary>
    /// <param name="databaseName">
    /// The name of the database to be created. Must be a valid SQL Server database name.
    /// The name should follow SQL Server naming conventions and should not contain special characters.
    /// </param>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server where the database will be created.
    /// This ID must correspond to an existing database server in the system.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type. This determines the category and intended use of the database.
    /// </param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server.
    /// If set to false, the database will only be registered in the system without being physically created.
    /// </param>
    /// <param name="useTrustedConnection">
    /// A flag indicating whether to use Windows authentication for the database connection.
    /// If set to true, the database will be configured to use Windows authentication instead of SQL Server authentication.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created database.
    /// This ID can be used to reference the database in future operations.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database server or type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database name is null, empty, or invalid, or when attempting to create a database with invalid parameters.</exception>
    /// <exception cref="ValidationException">Thrown when the database name violates business validation rules or when the database configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database with the same name already exists on the server.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database server or creating the database.</exception>
    public async Task<int> CreateDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool affectDatabase = true,
        bool useTrustedConnection = false
    )
    {
        return await _databaseService.CreateDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            Status.Active,
            affectDatabase,
            useTrustedConnection
        );
    }

    /// <summary>
    /// Retrieves a database by its unique identifier.
    /// This method returns detailed information about a specific database, including its configuration,
    /// status, and associated metadata.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be retrieved.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <returns>
    /// A <see cref="Database"/> object containing detailed information about the database,
    /// including its name, server, type, status, and configuration settings.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database ID is invalid.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the database information.</exception>
    public async Task<Database> GetDatabase(int databaseId)
    {
        return await _databaseService.GetDatabase(databaseId);
    }

    /// <summary>
    /// Retrieves a list of all available databases in the system.
    /// This method returns comprehensive information about all databases, including their configuration,
    /// status, and associated metadata. The list can be used for administrative purposes or to audit
    /// the system's database configuration.
    /// </summary>
    /// <returns>
    /// A list of <see cref="Database"/> objects, each containing detailed information about a database,
    /// including its name, server, type, status, and configuration settings.
    /// </returns>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the database list.</exception>
    public async Task<List<Database>> GetDatabases()
    {
        return await _databaseService.GetDatabases();
    }

    /// <summary>
    /// Deletes a database from the system without physical removal.
    /// This method removes the database from the DbLocator system while keeping the
    /// physical database intact on the server. This is useful for temporarily removing
    /// a database from the system without deleting its data.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be deleted.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// the database has been successfully removed from the system.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database does not exist in the system.</exception>
    /// <exception cref="ValidationException">Thrown when the deletion violates validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database is in use or has active connections.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the deletion operation fails.</exception>
    public async Task DeleteDatabase(int databaseId)
    {
        await _databaseService.DeleteDatabase(databaseId, false);
    }

    /// <summary>
    /// Deletes a database with optional physical removal.
    /// This method allows for either removing the database from the DbLocator system only,
    /// or completely removing it from both the system and the physical server. The operation
    /// includes validation to ensure the deletion is safe and does not affect other system components.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be deleted.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="deleteDatabase">
    /// A flag indicating whether to physically delete the database from the server.
    /// When true, the database will be completely removed from both the system and the server.
    /// When false, the database will only be removed from the DbLocator system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// the database has been successfully deleted according to the specified options.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database does not exist in the system.</exception>
    /// <exception cref="ValidationException">Thrown when the deletion violates validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database is in use, has active connections, or when physical deletion is not allowed.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the deletion operation fails.</exception>
    public async Task DeleteDatabase(int databaseId, bool deleteDatabase)
    {
        await _databaseService.DeleteDatabase(databaseId, deleteDatabase);
    }

#nullable enable
    /// <summary>
    /// Updates an existing database with the specified configuration.
    /// This method allows updating all properties of a database, including its name, server, type,
    /// trusted connection setting, and status. Only the provided parameters will be updated; others will remain unchanged.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be updated. This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="databaseName">
    /// The new name for the database. If null, the name will not be changed.
    /// </param>
    /// <param name="databaseServerId">
    /// The unique identifier of the database server where the database will be moved. If null, the server will not be changed.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type. If null, the type will not be changed.
    /// </param>
    /// <param name="useTrustedConnection">
    /// A flag indicating whether to use Windows authentication for the database connection. If null, this setting will not be changed.
    /// </param>
    /// <param name="status">
    /// The new status of the database (e.g., Active, Inactive). If null, the status will not be changed.
    /// </param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server.
    /// If set to false, the database will only be registered in the system without being physically created.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the database has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the parameters are invalid or violate business rules.</exception>
    /// <exception cref="ValidationException">Thrown when the update violates validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the update operation is not allowed.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.</exception>
    public async Task UpdateDatabase(
        int databaseId,
        string? databaseName,
        int? databaseServerId,
        byte? databaseTypeId,
        bool? useTrustedConnection,
        Status? status,
        bool? affectDatabase
    )
    {
        await _databaseService.UpdateDatabase(
            databaseId,
            databaseName,
            databaseServerId,
            databaseTypeId,
            useTrustedConnection,
            status,
            affectDatabase
        );
    }
}
