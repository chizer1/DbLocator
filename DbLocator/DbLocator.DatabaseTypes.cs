using DbLocator.Domain;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    ///Add database type
    /// </summary>
    /// <param name="databaseTypeName"></param>
    /// <returns>DatabaseTypeId</returns>
    public async Task<byte> AddDatabaseType(string databaseTypeName)
    {
        return await _databaseTypes.AddDatabaseType(databaseTypeName);
    }

    /// <summary>
    ///Get database types
    /// </summary>
    /// <returns>List of database types</returns>
    public async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        return await _databaseTypes.GetDatabaseTypes();
    }

    /// <summary>
    ///Update database type
    /// </summary>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseTypeName"></param>
    /// <returns></returns>
    public async Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName)
    {
        await _databaseTypes.UpdateDatabaseType(databaseTypeId, databaseTypeName);
    }

    /// <summary>
    ///Delete database type
    /// </summary>
    /// <param name="databaseTypeId"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseType(byte databaseTypeId)
    {
        await _databaseTypes.DeleteDatabaseType(databaseTypeId);
    }
}
