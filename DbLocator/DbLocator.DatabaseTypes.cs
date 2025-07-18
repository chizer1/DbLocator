using DbLocator.Domain;
using FluentValidation;
using Microsoft.Data.SqlClient;

namespace DbLocator;

/// <summary>
/// This partial class contains all database type-related operations for the DbLocator system.
/// It provides comprehensive methods for managing database types, including:
/// - Creation of new database types
/// - Retrieval of database type information
/// - Updates to database type settings
/// - Deletion of database types
/// </summary>
public partial class Locator
{
    /// <summary>
    /// Creates a new database type in the system.
    /// This method establishes a new database type that can be used to categorize and manage different categories
    /// </summary>
    /// <param name="databaseTypeName">
    /// The name of the database type to be created. This should be a descriptive name that identifies its purpose (e.g., Operational, Analytical, Reporting).
    /// The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created database type. This ID can be used to reference
    /// the database type in future operations and is used internally by the system.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the database type name is null, empty, or invalid.
    /// The name must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the database type name violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database type with the same name already exists.
    /// Database type names must be unique across the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or creating the database type record.</exception>
    public async Task<byte> CreateDatabaseType(string databaseTypeName)
    {
        return await _databaseTypeService.CreateDatabaseType(databaseTypeName);
    }

    /// <summary>
    /// Retrieves a single database type by its unique identifier.
    /// This method returns information about a specific database type.
    /// </summary>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type to retrieve. This ID must correspond to an
    /// existing database type in the system.
    /// </param>
    /// <returns>
    /// A <see cref="DatabaseType"/> object containing detailed information about the database type,
    /// including its name, configuration settings, and associated metadata.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database type is found with the given ID.
    /// This indicates that the database type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database type ID is invalid.
    /// The ID must be a valid byte value.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the database type information.</exception>
    public async Task<DatabaseType> GetDatabaseType(byte databaseTypeId)
    {
        return await _databaseTypeService.GetDatabaseType(databaseTypeId);
    }

    /// <summary>
    /// Retrieves a list of all available database types in the system.
    /// This method returns comprehensive information about all database types, including their
    /// configuration and associated metadata.
    /// </summary>
    /// <returns>
    /// A list of <see cref="DatabaseType"/> objects, each containing detailed information about a database type,
    /// including its name, configuration settings, and associated metadata.
    /// </returns>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the database type list.</exception>
    public async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        return await _databaseTypeService.GetDatabaseTypes();
    }

    /// <summary>
    /// Updates the name of an existing database type.
    /// This method allows changing the display name of a database type while preserving its other settings.
    /// The operation is performed asynchronously and updates only the database type's name.
    /// </summary>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type to be updated. This ID must correspond to an
    /// existing database type in the system.
    /// </param>
    /// <param name="databaseTypeName">
    /// The new name for the database type. This should be a descriptive name that identifies
    /// its purpose. The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the database type
    /// has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database type is found with the given ID.
    /// This indicates that the database type does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the database type name is null, empty, or invalid.
    /// The name must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the database type name violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a database type with the same name already exists.
    /// Database type names must be unique across the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or updating the database type.</exception>
    public async Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName)
    {
        await _databaseTypeService.UpdateDatabaseType(databaseTypeId, databaseTypeName);
    }

    /// <summary>
    /// Deletes a database type by its unique identifier.
    /// This method permanently removes a database type from the system. The operation is irreversible
    /// and will remove all associated database type data. Before deletion, ensure that the database type
    /// is not in use by any databases in the system.
    /// </summary>
    /// <param name="databaseTypeId">
    /// The unique identifier of the database type to be deleted. This ID must correspond to an
    /// existing database type in the system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the database type
    /// has been successfully deleted.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database type is found with the given ID.
    /// This indicates that the database type does not exist in the system.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to delete a database type that is in use
    /// by any databases. These databases must be updated or deleted before the database type can be removed.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or deleting the database type.</exception>
    public async Task DeleteDatabaseType(byte databaseTypeId)
    {
        await _databaseTypeService.DeleteDatabaseType(databaseTypeId);
    }
}
