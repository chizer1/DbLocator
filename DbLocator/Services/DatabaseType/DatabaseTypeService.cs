using DbLocator.Db;
using DbLocator.Features.DatabaseTypes.CreateDatabaseType;
using DbLocator.Features.DatabaseTypes.DeleteDatabaseType;
using DbLocator.Features.DatabaseTypes.GetDatabaseTypeById;
using DbLocator.Features.DatabaseTypes.GetDatabaseTypes;
using DbLocator.Features.DatabaseTypes.UpdateDatabaseType;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseType;

internal class DatabaseTypeService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : IDatabaseTypeService
{
    private readonly CreateDatabaseTypeHandler _createDatabaseType = new(dbContextFactory, cache);
    private readonly DeleteDatabaseTypeHandler _deleteDatabaseType = new(dbContextFactory, cache);
    private readonly GetDatabaseTypesHandler _getDatabaseTypes = new(dbContextFactory, cache);
    private readonly GetDatabaseTypeByIdHandler _getDatabaseTypeById = new(dbContextFactory, cache);
    private readonly UpdateDatabaseTypeHandler _updateDatabaseType = new(dbContextFactory, cache);

    public async Task<byte> AddDatabaseType(string databaseTypeName)
    {
        return await _createDatabaseType.Handle(new CreateDatabaseTypeCommand(databaseTypeName));
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
        return await _getDatabaseTypeById.Handle(new GetDatabaseTypeByIdQuery(databaseTypeId));
    }

    public async Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName)
    {
        await _updateDatabaseType.Handle(
            new UpdateDatabaseTypeCommand(databaseTypeId, databaseTypeName)
        );
    }
}
