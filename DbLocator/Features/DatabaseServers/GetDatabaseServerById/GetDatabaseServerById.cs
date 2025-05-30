#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.GetDatabaseServerById;

internal record GetDatabaseServerByIdQuery(int DatabaseServerId);

internal sealed class GetDatabaseServerByIdQueryValidator
    : AbstractValidator<GetDatabaseServerByIdQuery>
{
    internal GetDatabaseServerByIdQueryValidator()
    {
        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database Server Id must be greater than 0.");
    }
}

internal class GetDatabaseServerByIdHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<DatabaseServer> Handle(
        GetDatabaseServerByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabaseServerByIdQueryValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        const string cacheKeyPrefix = "databaseServer-id-";
        var cacheKey = $"{cacheKeyPrefix}{request.DatabaseServerId}";

        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<DatabaseServer>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseServerEntity =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(
                    ds => ds.DatabaseServerId == request.DatabaseServerId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database Server with ID {request.DatabaseServerId} not found."
            );

        var result = new DatabaseServer(
            databaseServerEntity.DatabaseServerId,
            databaseServerEntity.DatabaseServerName,
            databaseServerEntity.DatabaseServerIpaddress,
            databaseServerEntity.DatabaseServerHostName,
            databaseServerEntity.DatabaseServerFullyQualifiedDomainName,
            databaseServerEntity.IsLinkedServer
        );

        if (_cache != null)
            await _cache.CacheData(cacheKey, result);

        return result;
    }
}
