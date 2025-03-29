using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseServers;

internal class GetDatabaseServersQuery { }

internal sealed class GetDatabaseServersQueryValidator : AbstractValidator<GetDatabaseServersQuery>
{
    internal GetDatabaseServersQueryValidator() { }
}

internal class GetDatabaseServers(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task<List<DatabaseServer>> Handle(GetDatabaseServersQuery query)
    {
        await new GetDatabaseServersQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databaseServers";
        var cachedData = await cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<DatabaseServer>>(cachedData);
        }

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServerEntities = await dbContext.Set<DatabaseServerEntity>().ToListAsync();

        var databaseServers = databaseServerEntities
            .Select(ds => new DatabaseServer(
                ds.DatabaseServerId,
                ds.DatabaseServerName,
                ds.DatabaseServerIpaddress,
                ds.DatabaseServerHostName,
                ds.DatabaseServerFullyQualifiedDomainName,
                ds.IsLinkedServer
            ))
            .ToList();

        var serializedData = System.Text.Json.JsonSerializer.Serialize(databaseServers);
        await cache.SetStringAsync(cacheKey, serializedData);

        return databaseServers;
    }
}
