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
        var cachedData = await cache?.GetCachedData<List<DatabaseServer>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseServers = await GetDatabaseServersFromDatabase(dbContextFactory);
        await cache?.CacheData(cacheKey, databaseServers);

        return databaseServers;
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
