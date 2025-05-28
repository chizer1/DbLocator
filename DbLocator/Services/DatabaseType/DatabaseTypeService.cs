using DbLocator.Db;
using DbLocator.Features.DatabaseTypes;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseType;

internal class DatabaseTypeService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : IDatabaseTypeService
{
    private readonly AddDatabaseType _addDatabaseType = new(dbContextFactory, cache);
    private readonly DeleteDatabaseType _deleteDatabaseType = new(dbContextFactory, cache);
    private readonly GetDatabaseTypes _getDatabaseTypes = new(dbContextFactory, cache);
    private readonly GetDatabaseType _getDatabaseType = new(dbContextFactory, cache);
    private readonly UpdateDatabaseType _updateDatabaseType = new(dbContextFactory, cache);

    public async Task<byte> AddDatabaseType(string databaseTypeName)
    {
        return await _addDatabaseType.Handle(new AddDatabaseTypeCommand(databaseTypeName));
    }

    public async Task DeleteDatabaseType(byte databaseTypeId)
    {
        await _deleteDatabaseType.Handle(new DeleteDatabaseTypeCommand(databaseTypeId));
    }

    public async Task<List<Domain.DatabaseType>> GetDatabaseTypes()
    {
        return await _getDatabaseTypes.Handle(new GetDatabaseTypesQuery());
    }

    public async Task<Domain.DatabaseType> GetDatabaseType(byte databaseTypeId)
    {
        return await _getDatabaseType.Handle(
            new GetDatabaseTypeQuery { DatabaseTypeId = databaseTypeId }
        );
    }

    public async Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName)
    {
        await _updateDatabaseType.Handle(
            new UpdateDatabaseTypeCommand(databaseTypeId, databaseTypeName)
        );
    }
}
