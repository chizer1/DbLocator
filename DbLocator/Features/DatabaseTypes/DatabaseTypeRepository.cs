using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes;

internal class DatabaseTypeRepository(IDbContextFactory<DbLocatorContext> dbContextFactory)
    : IDatabaseTypeRepository
{
    public async Task<int> AddDatabaseType(string databaseTypeName)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseType = new DatabaseTypeEntity { DatabaseTypeName = databaseTypeName };
        dbContext.Add(databaseType);
        await dbContext.SaveChangesAsync();
        return databaseType.DatabaseTypeId;
    }

    public async Task<DatabaseType> GetDatabaseType(int databaseTypeId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        return await dbContext
            .Set<DatabaseType>()
            .FirstOrDefaultAsync(dt => dt.Id == databaseTypeId);
    }

    public async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseTypeEntities = await dbContext.Set<DatabaseTypeEntity>().ToListAsync();

        return databaseTypeEntities
            .Select(entity => new DatabaseType(entity.DatabaseTypeId, entity.DatabaseTypeName))
            .ToList();
    }

    public async Task UpdateDatabaseType(int databaseTypeId, string databaseTypeName)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseType = await dbContext
            .Set<DatabaseTypeEntity>()
            .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == databaseTypeId);

        if (databaseType != null)
        {
            databaseType.DatabaseTypeName = databaseTypeName;
            dbContext.Update(databaseType);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteDatabaseType(int databaseTypeId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseType = await dbContext
            .Set<DatabaseTypeEntity>()
            .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == databaseTypeId);

        if (databaseType != null)
        {
            dbContext.Remove(databaseType);
            await dbContext.SaveChangesAsync();
        }
    }
}
