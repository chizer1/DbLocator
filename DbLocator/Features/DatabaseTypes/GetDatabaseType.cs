using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes;

internal class GetDatabaseTypeQuery
{
    public byte DatabaseTypeId { get; set; }
}

internal sealed class GetDatabaseTypeQueryValidator : AbstractValidator<GetDatabaseTypeQuery>
{
    internal GetDatabaseTypeQueryValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
    }
}

internal class GetDatabaseType(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<DatabaseType> Handle(GetDatabaseTypeQuery query)
    {
        await new GetDatabaseTypeQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = $"databaseType-{query.DatabaseTypeId}";
        var cachedData = await cache?.GetCachedData<DatabaseType>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseType = await GetDatabaseTypeFromDatabase(
            dbContextFactory,
            query.DatabaseTypeId
        );
        await cache?.CacheData(cacheKey, databaseType);

        return databaseType;
    }

    private static async Task<DatabaseType> GetDatabaseTypeFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        byte databaseTypeId
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseTypeEntity =
            await dbContext
                .Set<DatabaseTypeEntity>()
                .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == databaseTypeId)
            ?? throw new KeyNotFoundException($"Database type with ID {databaseTypeId} not found.");

        return new DatabaseType(
            databaseTypeEntity.DatabaseTypeId,
            databaseTypeEntity.DatabaseTypeName
        );
    }
}
