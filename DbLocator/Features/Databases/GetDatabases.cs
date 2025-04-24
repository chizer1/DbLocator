using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal class GetDatabasesQuery { }

internal sealed class GetDatabasesQueryValidator : AbstractValidator<GetDatabasesQuery>
{
    internal GetDatabasesQueryValidator() { }
}

internal class GetDatabases(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    public async Task<List<Database>> Handle(GetDatabasesQuery query)
    {
        await new GetDatabasesQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databases";
        var cachedData = await cache?.GetCachedData<List<Database>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databases = await GetDatabasesFromDatabase(dbContextFactory);
        await cache?.CacheData(cacheKey, databases);

        return databases;
    }

    private static async Task<List<Database>> GetDatabasesFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseEntities = await dbContext
            .Set<DatabaseEntity>()
            .AsNoTracking()
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
