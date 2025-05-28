#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes.GetDatabaseTypeById;

internal record GetDatabaseTypeByIdQuery(byte DatabaseTypeId);

internal sealed class GetDatabaseTypeByIdQueryValidator
    : AbstractValidator<GetDatabaseTypeByIdQuery>
{
    internal GetDatabaseTypeByIdQueryValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
    }
}

internal class GetDatabaseTypeByIdHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<DatabaseType> Handle(
        GetDatabaseTypeByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabaseTypeByIdQueryValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        const string cacheKeyPrefix = "databaseType-";
        var cacheKey = $"{cacheKeyPrefix}{request.DatabaseTypeId}";

        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<DatabaseType>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseTypeEntity =
            await dbContext
                .Set<DatabaseTypeEntity>()
                .FirstOrDefaultAsync(
                    dt => dt.DatabaseTypeId == request.DatabaseTypeId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database type with ID {request.DatabaseTypeId} not found."
            );

        var result = new DatabaseType(
            databaseTypeEntity.DatabaseTypeId,
            databaseTypeEntity.DatabaseTypeName
        );

        if (_cache != null)
            await _cache.CacheData(cacheKey, result);

        return result;
    }
}

#nullable disable
