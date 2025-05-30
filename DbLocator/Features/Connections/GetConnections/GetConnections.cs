#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections.GetConnections;

internal record GetConnectionsQuery;

internal sealed class GetConnectionsQueryValidator : AbstractValidator<GetConnectionsQuery>
{
    internal GetConnectionsQueryValidator() { }
}

internal class GetConnectionsHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<IEnumerable<Connection>> Handle(
        GetConnectionsQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetConnectionsQueryValidator().ValidateAndThrowAsync(request, cancellationToken);

        const string cacheKey = "connections";

        if (_cache != null)
        {
            var cachedConnections = await _cache.GetCachedData<IEnumerable<Connection>>(cacheKey);
            if (cachedConnections != null)
                return cachedConnections;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var connections = await dbContext
            .Set<ConnectionEntity>()
            .Include(c => c.Tenant)
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseType)
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Select(c => new Connection(
                c.ConnectionId,
                new Database(
                    c.Database.DatabaseId,
                    c.Database.DatabaseName,
                    new DatabaseType(
                        c.Database.DatabaseType.DatabaseTypeId,
                        c.Database.DatabaseType.DatabaseTypeName
                    ),
                    new DatabaseServer(
                        c.Database.DatabaseServer.DatabaseServerId,
                        c.Database.DatabaseServer.DatabaseServerName,
                        c.Database.DatabaseServer.DatabaseServerIpaddress,
                        c.Database.DatabaseServer.DatabaseServerHostName,
                        c.Database.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                        c.Database.DatabaseServer.IsLinkedServer
                    ),
                    (Status)c.Database.DatabaseStatusId,
                    c.Database.UseTrustedConnection
                ),
                new Tenant(
                    c.Tenant.TenantId,
                    c.Tenant.TenantName,
                    c.Tenant.TenantCode,
                    (Status)c.Tenant.TenantStatusId
                )
            ))
            .ToListAsync(cancellationToken);

        if (_cache != null)
            await _cache.CacheData(cacheKey, connections);

        return connections;
    }
}
