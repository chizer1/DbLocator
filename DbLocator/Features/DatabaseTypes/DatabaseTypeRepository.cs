using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes;

internal class DatabaseTypeRepository(DbContext DbLocatorDb) : IDatabaseTypeRepository
{
    public async Task<int> AddDatabaseType(string databaseTypeName)
    {
        var databaseType = new DatabaseTypeEntity { DatabaseTypeName = databaseTypeName };
        DbLocatorDb.Add(databaseType);
        await DbLocatorDb.SaveChangesAsync();
        return databaseType.DatabaseTypeId;
    }

    public async Task<DatabaseType> GetDatabaseType(int databaseTypeId)
    {
        return await DbLocatorDb
            .Set<DatabaseType>()
            .FirstOrDefaultAsync(dt => dt.Id == databaseTypeId);
    }

    public async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        var databaseTypeEntities = await DbLocatorDb.Set<DatabaseTypeEntity>().ToListAsync();

        return databaseTypeEntities
            .Select(entity => new DatabaseType(entity.DatabaseTypeId, entity.DatabaseTypeName))
            .ToList();
    }

    public async Task UpdateDatabaseType(int databaseTypeId, string databaseTypeName)
    {
        var databaseType = await DbLocatorDb
            .Set<DatabaseTypeEntity>()
            .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == databaseTypeId);

        if (databaseType != null)
        {
            databaseType.DatabaseTypeName = databaseTypeName;
            DbLocatorDb.Update(databaseType);
            await DbLocatorDb.SaveChangesAsync();
        }
    }

    public async Task DeleteDatabaseType(int databaseTypeId)
    {
        var databaseType = await DbLocatorDb
            .Set<DatabaseTypeEntity>()
            .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == databaseTypeId);

        if (databaseType != null)
        {
            DbLocatorDb.Remove(databaseType);
            await DbLocatorDb.SaveChangesAsync();
        }
    }
}
