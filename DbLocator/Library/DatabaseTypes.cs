using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Library;

internal class DatabaseTypes(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    private readonly AddDatabaseType _addDatabaseType = new(dbContextFactory, cache);
    private readonly DeleteDatabaseType _deleteDatabaseType = new(dbContextFactory, cache);
    private readonly GetDatabaseTypes _getDatabaseTypes = new(dbContextFactory, cache);
    private readonly UpdateDatabaseType _updateDatabaseType = new(dbContextFactory, cache);

    internal async Task<byte> AddDatabaseType(string databaseTypeName)
    {
        return await _addDatabaseType.Handle(new AddDatabaseTypeCommand(databaseTypeName));
    }

    internal async Task DeleteDatabaseType(byte databaseTypeId)
    {
        await _deleteDatabaseType.Handle(new DeleteDatabaseTypeCommand(databaseTypeId));
    }

    internal async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        return await _getDatabaseTypes.Handle(new GetDatabaseTypesQuery());
    }

    internal async Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName)
    {
        await _updateDatabaseType.Handle(
            new UpdateDatabaseTypeCommand(databaseTypeId, databaseTypeName)
        );
    }
}
