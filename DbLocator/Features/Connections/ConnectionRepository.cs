using System.Data.SqlClient;
using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections;

internal class ConnectionRepository(IDbContextFactory<DbLocatorContext> dbContextFactory)
    : IConnectionRepository
{
    public async Task<int> AddConnection(int tenantId, int databaseId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var connection = new ConnectionEntity { TenantId = tenantId, DatabaseId = databaseId, };

        await dbContext.Set<ConnectionEntity>().AddAsync(connection);
        await dbContext.SaveChangesAsync();

        return connection.ConnectionId;
    }

    public async Task<SqlConnection> GetSqlConnection(int tenantId, int databaseTypeId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var connectionEntity =
            await dbContext
                .Set<ConnectionEntity>()
                .Include(con => con.Database)
                .Where(con => con.Database.DatabaseTypeId == databaseTypeId)
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Connection not found.");

        var database =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(d => d.DatabaseId == connectionEntity.DatabaseId)
            ?? throw new KeyNotFoundException(
                $"Database with ID {connectionEntity.DatabaseId} not found."
            );

        var connectionString = database.UseTrustedConnection
            ? $"Server={database.DatabaseServer.DatabaseServerName};Database={database.DatabaseName};Trusted_Connection=True;"
            : $"Server={database.DatabaseServer.DatabaseServerName};Database={database.DatabaseName};User Id={database.DatabaseUser};Password={database.DatabaseUserPassword};";

        return new SqlConnection(connectionString);
    }

    public async Task<List<Connection>> GetConnections()
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var connectionEntities = await dbContext
            .Set<ConnectionEntity>()
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(c => c.Tenant)
            .ToListAsync();

        return connectionEntities
            .Select(connectionEntity => new Connection(
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
            .ToList();
    }

    public async Task DeleteConnection(int connectionId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var connection =
            await dbContext
                .Set<ConnectionEntity>()
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId)
            ?? throw new KeyNotFoundException($"Connection with ID {connectionId} not found.");

        dbContext.Set<ConnectionEntity>().Remove(connection);
        await dbContext.SaveChangesAsync();
    }
}
