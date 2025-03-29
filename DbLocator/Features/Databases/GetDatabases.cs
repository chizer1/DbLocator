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
        var cachedDatabases = await cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedDatabases))
            return JsonSerializer.Deserialize<List<Database>>(cachedDatabases);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseEntities = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseServer)
            .Include(d => d.DatabaseType)
            .ToListAsync();

        var databases = databaseEntities
            .Select(d => new Database(
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
            .ToList();

        var serializedDatabases = JsonSerializer.Serialize(databases);
        await cache.SetStringAsync(cacheKey, serializedDatabases);

        return databases;
    }
}
