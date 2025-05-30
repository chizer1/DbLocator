using DbLocator.Domain;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    /// Creates a new database type.
    /// </summary>
    /// <param name="databaseTypeName">
    /// The name of the database type to be Createed.
    /// </param>
    /// <returns>
    /// The ID of the newly Createed database type.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the database type name is invalid or already exists. The parameter name is included.
    /// </exception>
    public async Task<byte> CreateDatabaseType(string databaseTypeName)
    {
        return await _databaseTypeService.CreateDatabaseType(databaseTypeName);
    }

    /// <summary>
    /// Retrieves a list of all available database types.
    /// </summary>
    /// <returns>
    /// A list of <see cref="DatabaseType"/> representing the available database types.
    /// </returns>
    public async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        return await _databaseTypeService.GetDatabaseTypes();
    }

    /// <summary>
    /// Updates an existing database type.
    /// </summary>
    /// <param name="databaseTypeId">
    /// The ID of the database type to be updated.
    /// </param>
    /// <param name="databaseTypeName">
    /// The new name for the database type.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the specified database type is not found.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the new database type name is invalid or already exists.
    /// </exception>
    public async Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName)
    {
        await _databaseTypeService.UpdateDatabaseType(databaseTypeId, databaseTypeName);
    }

    /// <summary>
    /// Deletes a specified database type.
    /// </summary>
    /// <param name="databaseTypeId">
    /// The ID of the database type to be deleted.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the specified database type is not found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to delete a database type that is in use.
    /// </exception>
    public async Task DeleteDatabaseType(byte databaseTypeId)
    {
        await _databaseTypeService.DeleteDatabaseType(databaseTypeId);
    }

    /// <summary>
    /// Retrieves a single database type by its ID.
    /// </summary>
    /// <param name="databaseTypeId">
    /// The ID of the database type to retrieve.
    /// </param>
    /// <returns>
    /// A <see cref="DatabaseType"/> object representing the database type with the specified ID.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no database type is found with the given ID.
    /// </exception>
    public async Task<DatabaseType> GetDatabaseType(byte databaseTypeId)
    {
        return await _databaseTypeService.GetDatabaseType(databaseTypeId);
    }
}
