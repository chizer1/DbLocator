using DbLocator.Domain;

namespace DbLocator.Features.DatabaseTypes;

internal interface IDatabaseTypeRepository
{
    public Task<int> AddDatabaseType(string databaseTypeName);
    public Task<DatabaseType> GetDatabaseType(int databaseTypeId);
    public Task<List<DatabaseType>> GetDatabaseTypes();
    public Task UpdateDatabaseType(int databaseTypeId, string databaseTypeName);
    public Task DeleteDatabaseType(int databaseTypeId);
}
