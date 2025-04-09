using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections;

internal class GetConnectionsQuery { }

internal sealed class GetConnectionsQueryValidator : AbstractValidator<GetConnectionsQuery>
{
    internal GetConnectionsQueryValidator() { }
}

internal class GetConnections(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<List<Connection>> Handle(GetConnectionsQuery query)
    {
        await new GetConnectionsQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "connections";
        var cachedData = await GetCachedData(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return DeserializeCachedData(cachedData);

        var connections = await GetConnectionsFromDatabase(dbContextFactory);
        await CacheData(cacheKey, connections);

        return connections;
    }

    private async Task<string> GetCachedData(string cacheKey)
    {
        return cache != null ? await cache.GetCachedData<string>(cacheKey) : null;
    }

    private static List<Connection> DeserializeCachedData(string cachedData)
    {
        return JsonSerializer.Deserialize<List<Connection>>(cachedData) ?? new List<Connection>();
    }

    private async Task CacheData(string cacheKey, List<Connection> connections)
    {
        if (cache != null)
        {
            var serializedData = JsonSerializer.Serialize(connections);
            await cache.CacheData(cacheKey, serializedData);
        }
    }

    private static async Task<List<Connection>> GetConnectionsFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var connectionEntities = await dbContext
            .Set<ConnectionEntity>()
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(c => c.Tenant)
            .ToListAsync();

        return
        [
            .. connectionEntities.Select(connectionEntity => new Connection(
                connectionEntity.ConnectionId,
                connectionEntity.Database != null
                    ? new Database(
                        connectionEntity.Database.DatabaseId,
                        connectionEntity.Database.DatabaseName,
                        connectionEntity.Database.DatabaseType != null
                            ? new DatabaseType(
                                connectionEntity.Database.DatabaseType.DatabaseTypeId,
                                connectionEntity.Database.DatabaseType.DatabaseTypeName
                            )
                            : null,
                        connectionEntity.Database.DatabaseServer != null
                            ? new DatabaseServer(
                                connectionEntity.Database.DatabaseServer.DatabaseServerId,
                                connectionEntity.Database.DatabaseServer.DatabaseServerName,
                                connectionEntity.Database.DatabaseServer.DatabaseServerIpaddress,
                                connectionEntity.Database.DatabaseServer.DatabaseServerHostName,
                                connectionEntity
                                    .Database
                                    .DatabaseServer
                                    .DatabaseServerFullyQualifiedDomainName,
                                connectionEntity.Database.DatabaseServer.IsLinkedServer
                            )
                            : null,
                        (Status)connectionEntity.Database.DatabaseStatusId,
                        connectionEntity.Database.UseTrustedConnection
                    )
                    : null,
                connectionEntity.Tenant != null
                    ? new Tenant(
                        connectionEntity.Tenant.TenantId,
                        connectionEntity.Tenant.TenantName,
                        connectionEntity.Tenant.TenantCode,
                        (Status)connectionEntity.Tenant.TenantStatusId
                    )
                    : null
            ))
        ];
    }
}
