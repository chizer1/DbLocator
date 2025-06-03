#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.GetDatabaseServers;

internal record GetDatabaseServersQuery;

internal sealed class GetDatabaseServersQueryValidator : AbstractValidator<GetDatabaseServersQuery>
{
    internal GetDatabaseServersQueryValidator() { }
}

internal class GetDatabaseServersHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<IEnumerable<DatabaseServer>> Handle(
        GetDatabaseServersQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabaseServersQueryValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        const string cacheKey = "databaseServers";

        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<IEnumerable<DatabaseServer>>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseServerEntities = await dbContext
            .Set<DatabaseServerEntity>()
            .ToListAsync(cancellationToken);

        var result = databaseServerEntities
            .Select(ds => new DatabaseServer(
                ds.DatabaseServerId,
                ds.DatabaseServerName,
                ds.DatabaseServerIpaddress,
                ds.DatabaseServerHostName,
                ds.DatabaseServerFullyQualifiedDomainName,
                ds.IsLinkedServer
            ))
            .ToList();

        if (_cache != null)
            await _cache.CacheData(cacheKey, result);

        return result;
    }
}
