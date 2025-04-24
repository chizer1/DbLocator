using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers;

internal class GetDatabaseServerQuery
{
    public int DatabaseServerId { get; set; }
}

internal sealed class GetDatabaseServerQueryValidator : AbstractValidator<GetDatabaseServerQuery>
{
    internal GetDatabaseServerQueryValidator()
    {
        RuleFor(x => x.DatabaseServerId).GreaterThan(0);
    }
}

internal class GetDatabaseServer(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<DatabaseServer> Handle(GetDatabaseServerQuery query)
    {
        await new GetDatabaseServerQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = $"databaseServer-{query.DatabaseServerId}";
        var cachedData = await cache?.GetCachedData<DatabaseServer>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseServer = await GetDatabaseServerFromDatabase(
            dbContextFactory,
            query.DatabaseServerId
        );
        await cache?.CacheData(cacheKey, databaseServer);

        return databaseServer;
    }

    private static async Task<DatabaseServer> GetDatabaseServerFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        int databaseServerId
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServerEntity =
            await dbContext
                .Set<DatabaseServerEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == databaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database server with ID {databaseServerId} not found."
            );

        return new DatabaseServer(
            databaseServerEntity.DatabaseServerId,
            databaseServerEntity.DatabaseServerName,
            databaseServerEntity.DatabaseServerIpaddress,
            databaseServerEntity.DatabaseServerHostName,
            databaseServerEntity.DatabaseServerFullyQualifiedDomainName,
            databaseServerEntity.IsLinkedServer
        );
    }
}
