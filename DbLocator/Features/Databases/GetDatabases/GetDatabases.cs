#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases.GetDatabases;

internal record GetDatabasesQuery;

internal sealed class GetDatabasesQueryValidator : AbstractValidator<GetDatabasesQuery>
{
    internal GetDatabasesQueryValidator()
    {
        // No validation rules needed for empty query
    }
}

internal class GetDatabasesHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<IEnumerable<Database>> Handle(
        GetDatabasesQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabasesQueryValidator().ValidateAndThrowAsync(request, cancellationToken);

        const string cacheKey = "databases";

        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<IEnumerable<Database>>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databases = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseType)
            .Include(d => d.DatabaseServer)
            .ToListAsync(cancellationToken);

        var result = databases
            .Select(d => new Database(
                d.DatabaseId,
                d.DatabaseName,
                new DatabaseType(d.DatabaseTypeId, d.DatabaseType.DatabaseTypeName),
                new DatabaseServer(
                    d.DatabaseServerId,
                    d.DatabaseServer.DatabaseServerName,
                    d.DatabaseServer.DatabaseServerHostName,
                    d.DatabaseServer.DatabaseServerIpaddress,
                    d.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                    d.DatabaseServer.IsLinkedServer
                ),
                (Status)d.DatabaseStatusId,
                d.UseTrustedConnection
            ))
            .ToList();

        if (_cache != null)
            await _cache.CacheData(cacheKey, result);

        return result;
    }
}

#nullable disable
