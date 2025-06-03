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
///
/// The database management system supports:
/// - Multiple database types (operational, analytical, etc.)
/// - Different database statuses (active, inactive, etc.)
/// - Trusted connection configurations
/// - Server-specific database placement
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
    /// Updates all properties of an existing database.
    /// This method allows for comprehensive modification of a database's configuration,
    /// including its name, server, type, and status. All changes are validated before
    /// being applied to ensure system integrity.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be updated.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="databaseName">
    /// The new name for the database. Must be a valid SQL Server database name.
    /// The name should follow SQL Server naming conventions and should not contain special characters.
    /// </param>
    /// <param name="databaseServerId">
    /// The unique identifier of the new database server. This ID must correspond to
    /// an existing database server in the system.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the new database type. This determines the category
    /// and intended use of the database.
    /// </param>
    /// <param name="databaseStatus">
    /// The new status for the database (e.g., Active, Inactive).
    /// This status determines the database's availability and usage.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// all database properties have been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database, server, or type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database name is null, empty, or invalid, or when attempting to update with invalid parameters.</exception>
    /// <exception cref="ValidationException">Thrown when the database name violates business validation rules or when the database configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database with the same name already exists on the server or when the update operation is not allowed.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.</exception>
    public async Task UpdateDatabase(
        int databaseId,
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        await _databaseService.UpdateDatabase(
            databaseId,
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus
        );
    }

    /// <summary>
    /// Updates the server assignment for an existing database.
    /// This method allows for moving a database to a different server while maintaining
    /// all other database properties. The operation includes validation to ensure the
    /// server change is valid and safe.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be updated.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="databaseServerId">
    /// The unique identifier of the new database server. This ID must correspond to
    /// an existing database server in the system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// the database server has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database or server does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the server ID is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the server change violates validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the server change is not allowed or when the database is in use.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.</exception>
    public async Task UpdateDatabase(int databaseId, int databaseServerId)
    {
        await _databaseService.UpdateDatabase(databaseId, databaseServerId);
    }

    /// <summary>
    /// Updates the type of an existing database.
    /// This method allows for changing the logical type of a database (e.g., from Operational
    /// to Analytical) while maintaining all other database properties. The operation includes
    /// validation to ensure the type change is valid and safe.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be updated.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="databaseTypeId">
    /// The unique identifier of the new database type. This determines the category
    /// and intended use of the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// the database type has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database or type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the type ID is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the type change violates validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the type change is not allowed or when the database is in use.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.</exception>
    public async Task UpdateDatabase(int databaseId, byte databaseTypeId)
    {
        await _databaseService.UpdateDatabase(databaseId, databaseTypeId);
    }

    /// <summary>
    /// Updates the name of an existing database.
    /// This method allows for renaming a database while maintaining all other database
    /// properties. The operation includes validation to ensure the new name is valid
    /// and not already in use.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be updated.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="databaseName">
    /// The new name for the database. Must be a valid SQL Server database name.
    /// The name should follow SQL Server naming conventions and should not contain special characters.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// the database name has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database name is null, empty, or invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the database name violates business validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database with the same name already exists or when the database is in use.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.</exception>
    public async Task UpdateDatabase(int databaseId, string databaseName)
    {
        await _databaseService.UpdateDatabase(databaseId, databaseName);
    }

    /// <summary>
    /// Updates the status of an existing database.
    /// This method allows for changing the operational status of a database (e.g., from
    /// Active to Inactive) while maintaining all other database properties. The operation
    /// includes validation to ensure the status change is valid and safe.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be updated.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="databaseStatus">
    /// The new status for the database (e.g., Active, Inactive).
    /// This status determines the database's availability and usage.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// the database status has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the status value is invalid.</exception>
    /// <exception cref="ValidationException">Thrown when the status change violates validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the status change is not allowed or when the database is in use.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.</exception>
    public async Task UpdateDatabase(int databaseId, Status databaseStatus)
    {
        await _databaseService.UpdateDatabase(databaseId, databaseStatus);
    }

    /// <summary>
    /// Updates the trusted connection setting for an existing database.
    /// This method allows for changing the authentication method of a database between
    /// Windows authentication and SQL Server authentication. The operation includes
    /// validation to ensure the change is valid and safe.
    /// </summary>
    /// <param name="databaseId">
    /// The unique identifier of the database to be updated.
    /// This ID must correspond to an existing database in the system.
    /// </param>
    /// <param name="useTrustedConnection">
    /// A flag indicating whether to use Windows authentication for the database connection.
    /// If set to true, the database will be configured to use Windows authentication instead
    /// of SQL Server authentication.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when
    /// the trusted connection setting has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database does not exist in the system.</exception>
    /// <exception cref="ValidationException">Thrown when the authentication change violates validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the authentication change is not allowed or when the database is in use.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the update operation fails.</exception>
    public async Task UpdateDatabase(int databaseId, bool useTrustedConnection)
    {
        await _databaseService.UpdateDatabase(databaseId, useTrustedConnection);
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
}
