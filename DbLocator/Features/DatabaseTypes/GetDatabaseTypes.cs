using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes;

internal class GetDatabaseTypesQuery { }

internal sealed class GetDatabaseTypesQueryValidator : AbstractValidator<GetDatabaseTypesQuery>
{
    internal GetDatabaseTypesQueryValidator() { }
}

internal class GetDatabaseTypes(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<List<DatabaseType>> Handle(GetDatabaseTypesQuery query)
    {
        await new GetDatabaseTypesQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databaseTypes";
        var cachedData = await cache?.GetCachedData<List<DatabaseType>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseTypes = await GetDatabaseTypesFromDatabase(dbContextFactory);
        await cache?.CacheData(cacheKey, databaseTypes);
        return databaseTypes;
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
