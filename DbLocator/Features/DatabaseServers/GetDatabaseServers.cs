using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers;

internal class GetDatabaseServersQuery { }

internal sealed class GetDatabaseServersQueryValidator : AbstractValidator<GetDatabaseServersQuery>
{
    internal GetDatabaseServersQueryValidator() { }
}

internal class GetDatabaseServers(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<List<DatabaseServer>> Handle(GetDatabaseServersQuery query)
    {
        await new GetDatabaseServersQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databaseServers";
        var cachedData = await GetCachedData(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return DeserializeCachedData(cachedData);

        var databaseServers = await GetDatabaseServersFromDatabase(dbContextFactory);
        await CacheData(cacheKey, databaseServers);

        return databaseServers;
    }

    private async Task<string> GetCachedData(string cacheKey)
    {
        return cache != null ? await cache.GetCachedData<string>(cacheKey) : null;
    }

    private static List<DatabaseServer> DeserializeCachedData(string cachedData)
    {
        return JsonSerializer.Deserialize<List<DatabaseServer>>(cachedData) ?? [];
    }

    private async Task CacheData(string cacheKey, List<DatabaseServer> databaseServers)
    {
        if (cache != null)
        {
            var serializedData = JsonSerializer.Serialize(databaseServers);
            await cache.CacheData(cacheKey, serializedData);
        }
    }

    private static async Task<List<DatabaseServer>> GetDatabaseServersFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServerEntities = await dbContext.Set<DatabaseServerEntity>().ToListAsync();

        return
        [
            .. databaseServerEntities.Select(ds => new DatabaseServer(
                ds.DatabaseServerId,
                ds.DatabaseServerName,
                ds.DatabaseServerIpaddress,
                ds.DatabaseServerHostName,
                ds.DatabaseServerFullyQualifiedDomainName,
                ds.IsLinkedServer
            ))
        ];
    }
}
