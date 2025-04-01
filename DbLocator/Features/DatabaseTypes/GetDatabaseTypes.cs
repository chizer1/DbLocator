using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseTypes;

internal class GetDatabaseTypesQuery { }

internal sealed class GetDatabaseTypesQueryValidator : AbstractValidator<GetDatabaseTypesQuery>
{
    internal GetDatabaseTypesQueryValidator() { }
}

internal class GetDatabaseTypes(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task<List<DatabaseType>> Handle(GetDatabaseTypesQuery query)
    {
        await new GetDatabaseTypesQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databaseTypes";
        var cachedData = await GetCachedData(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return DeserializeCachedData(cachedData);

        var databaseTypes = await GetDatabaseTypesFromDatabase(dbContextFactory);
        await CacheData(cacheKey, databaseTypes);

        return databaseTypes;
    }

    private async Task<string> GetCachedData(string cacheKey)
    {
        return cache != null ? await cache.GetStringAsync(cacheKey) : null;
    }

    private static List<DatabaseType> DeserializeCachedData(string cachedData)
    {
        return JsonSerializer.Deserialize<List<DatabaseType>>(cachedData) ?? [];
    }

    private async Task CacheData(string cacheKey, List<DatabaseType> databaseTypes)
    {
        if (cache != null)
        {
            var serializedData = JsonSerializer.Serialize(databaseTypes);
            await cache.SetStringAsync(cacheKey, serializedData);
        }
    }

    private static async Task<List<DatabaseType>> GetDatabaseTypesFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseTypeEntities = await dbContext.Set<DatabaseTypeEntity>().ToListAsync();

        return
        [
            .. databaseTypeEntities.Select(dt => new DatabaseType(
                dt.DatabaseTypeId,
                dt.DatabaseTypeName
            ))
        ];
    }
}
