using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.GetDatabaseServers;

/// <summary>
/// Handles the ListDatabaseServersQuery and returns all database servers.
/// </summary>
internal class GetDatabaseServersHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    public async Task<List<DatabaseServer>> Handle()
    {
        var cacheKey = "databaseServers";
        var cachedData = await cache?.GetCachedData<List<DatabaseServer>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseServers = await GetDatabaseServersFromDatabase();
        await cache?.CacheData(cacheKey, databaseServers);

        return databaseServers;
    }

    private async Task<List<DatabaseServer>> GetDatabaseServersFromDatabase()
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
