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

        await using var dbContext = dbContextFactory.CreateDbContext();

        var cacheKey = "databaseTypes";
        var cachedData = await cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return JsonSerializer.Deserialize<List<DatabaseType>>(cachedData);

        var databaseTypeEntities = await dbContext.Set<DatabaseTypeEntity>().ToListAsync();
        var databaseTypes = databaseTypeEntities
            .Select(entity => new DatabaseType(entity.DatabaseTypeId, entity.DatabaseTypeName))
            .ToList();

        var serializedData = JsonSerializer.Serialize(databaseTypes);
        await cache.SetStringAsync(cacheKey, serializedData);

        return databaseTypes;
    }
}
