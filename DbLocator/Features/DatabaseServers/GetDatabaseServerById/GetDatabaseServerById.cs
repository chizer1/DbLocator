using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.GetDatabaseServerById;

/// <summary>
/// Represents a query to get a database server by its ID.
/// </summary>
internal record GetDatabaseServerByIdQuery(int DatabaseServerId);

/// <summary>
/// Validates the GetDatabaseServerByIdQuery.
/// </summary>
internal sealed class GetDatabaseServerByIdQueryValidator
    : AbstractValidator<GetDatabaseServerByIdQuery>
{
    public GetDatabaseServerByIdQueryValidator()
    {
        RuleFor(x => x.DatabaseServerId).GreaterThan(0);
    }
}

/// <summary>
/// Handles the GetDatabaseServerByIdQuery and returns the corresponding database server.
/// </summary>
internal class GetDatabaseServerByIdHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    public async Task<DatabaseServer> Handle(GetDatabaseServerByIdQuery query)
    {
        await new GetDatabaseServerByIdQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = $"databaseServer-id-{query.DatabaseServerId}";
        var cachedData = await cache?.GetCachedData<DatabaseServer>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseServer = await GetDatabaseServerFromDatabase(query.DatabaseServerId);
        await cache?.CacheData(cacheKey, databaseServer);

        return databaseServer;
    }

    private async Task<DatabaseServer> GetDatabaseServerFromDatabase(int databaseServerId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServerEntity =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == databaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server with ID {databaseServerId} not found."
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
