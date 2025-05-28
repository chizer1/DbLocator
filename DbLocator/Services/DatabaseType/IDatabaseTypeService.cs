namespace DbLocator.Services.DatabaseType;

internal interface IDatabaseTypeService
{
    Task<byte> AddDatabaseType(string databaseTypeName);
    Task DeleteDatabaseType(byte databaseTypeId);
    Task<List<Domain.DatabaseType>> GetDatabaseTypes();
    Task<Domain.DatabaseType> GetDatabaseType(byte databaseTypeId);
    Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName);
}
