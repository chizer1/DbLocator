using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.Databases;

internal class GetDatabasesQuery { }

internal sealed class GetDatabasesQueryValidator : AbstractValidator<GetDatabasesQuery>
{
    internal GetDatabasesQueryValidator() { }
}

internal class GetDatabases(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    public async Task<List<Database>> Handle(GetDatabasesQuery query)
    {
        await new GetDatabasesQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databases";
        var cachedData = await GetCachedData(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return DeserializeCachedData(cachedData);

        var databases = await GetDatabasesFromDatabase(dbContextFactory);
        await CacheData(cacheKey, databases);

        return databases;
    }

    private async Task<string> GetCachedData(string cacheKey)
    {
        return cache != null ? await cache.GetStringAsync(cacheKey) : null;
    }

    private static List<Database> DeserializeCachedData(string cachedData)
    {
        return JsonSerializer.Deserialize<List<Database>>(cachedData) ?? [];
    }

    private async Task CacheData(string cacheKey, List<Database> databases)
    {
        if (cache != null)
        {
            var serializedData = JsonSerializer.Serialize(databases);
            await cache.SetStringAsync(cacheKey, serializedData);
        }
    }

    private static async Task<List<Database>> GetDatabasesFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseEntities = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseServer)
            .Include(d => d.DatabaseType)
            .ToListAsync();

        return
        [
            .. databaseEntities.Select(d => new Database(
                d.DatabaseId,
                d.DatabaseName,
                new DatabaseType(d.DatabaseType.DatabaseTypeId, d.DatabaseType.DatabaseTypeName),
                new DatabaseServer(
                    d.DatabaseServer.DatabaseServerId,
                    d.DatabaseServer.DatabaseServerName,
                    d.DatabaseServer.DatabaseServerIpaddress,
                    d.DatabaseServer.DatabaseServerHostName,
                    d.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                    d.DatabaseServer.IsLinkedServer
                ),
                (Status)d.DatabaseStatusId,
                d.UseTrustedConnection
            ))
        ];
    }
}
