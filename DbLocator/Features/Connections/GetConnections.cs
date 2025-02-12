using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections;

internal class GetConnectionsQuery { }

internal sealed class GetConnectionsQueryValidator : AbstractValidator<GetConnectionsQuery>
{
    internal GetConnectionsQueryValidator() { }
}

internal class GetConnections(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task<List<Connection>> Handle(GetConnectionsQuery query)
    {
        await new GetConnectionsQueryValidator().ValidateAndThrowAsync(query);

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
                        connectionEntity.Database.DatabaseUser,
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
                                connectionEntity.Database.DatabaseServer.DatabaseServerIpaddress
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
