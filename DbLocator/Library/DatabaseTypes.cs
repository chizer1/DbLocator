using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseTypes;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class DatabaseTypes(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    private readonly AddDatabaseType _addDatabaseType = new(dbContextFactory);
    private readonly DeleteDatabaseType _deleteDatabaseType = new(dbContextFactory);
    private readonly GetDatabaseTypes _getDatabaseTypes = new(dbContextFactory);
    private readonly UpdateDatabaseType _updateDatabaseType = new(dbContextFactory);

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
