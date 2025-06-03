#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes.GetDatabaseTypes;

internal record GetDatabaseTypesQuery;

internal sealed class GetDatabaseTypesQueryValidator : AbstractValidator<GetDatabaseTypesQuery>
{
    internal GetDatabaseTypesQueryValidator() { }
}

internal class GetDatabaseTypesHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<List<DatabaseType>> Handle(
        GetDatabaseTypesQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabaseTypesQueryValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        const string cacheKey = "databaseTypes";

        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<List<DatabaseType>>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseTypeEntities = await dbContext
            .Set<DatabaseTypeEntity>()
            .ToListAsync(cancellationToken);

        var result = databaseTypeEntities
            .Select(dt => new DatabaseType(dt.DatabaseTypeId, dt.DatabaseTypeName))
            .ToList();

        if (_cache != null)
            await _cache.CacheData(cacheKey, result);

        return result;
    }
}
