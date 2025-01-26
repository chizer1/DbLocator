using System.Data.SqlClient;
using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections;

internal class ConnectionRepository(DbContext DbLocatorDb) : IConnectionRepository
{
    public async Task<int> AddConnection(int tenantId, int databaseId)
    {
        var connection = new ConnectionEntity { TenantId = tenantId, DatabaseId = databaseId, };

        await DbLocatorDb.Set<ConnectionEntity>().AddAsync(connection);
        await DbLocatorDb.SaveChangesAsync();

        return connection.ConnectionId;
    }

    public async Task<SqlConnection> GetSqlConnection(int tenantId, int databaseTypeId)
    {
        var connectionEntity =
            await DbLocatorDb
                .Set<ConnectionEntity>()
                .Include(con => con.Database)
                .Where(con => con.Database.DatabaseTypeId == databaseTypeId)
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Connection not found.");

        var database =
            await DbLocatorDb
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
        var connectionEntities = await DbLocatorDb
            .Set<ConnectionEntity>()
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(c => c.Tenant)
            .ToListAsync();

        return
        [
            .. connectionEntities.Select(connectionEntity => new Connection(
                connectionEntity.ConnectionId,
                new Database(
                    connectionEntity.Database.DatabaseId,
                    connectionEntity.Database.DatabaseName,
                    new DatabaseType(
                        connectionEntity.Database.DatabaseType.DatabaseTypeId,
                        connectionEntity.Database.DatabaseType.DatabaseTypeName
                    ),
                    new DatabaseServer(
                        connectionEntity.Database.DatabaseServer.DatabaseServerId,
                        connectionEntity.Database.DatabaseServer.DatabaseServerName,
                        connectionEntity.Database.DatabaseServer.DatabaseServerIpaddress
                    ),
                    (Status)connectionEntity.Database.DatabaseStatusId,
                    connectionEntity.Database.UseTrustedConnection
                ),
                new Tenant(
                    connectionEntity.Tenant.TenantId,
                    connectionEntity.Tenant.TenantName,
                    connectionEntity.Tenant.TenantCode,
                    (Status)connectionEntity.Tenant.TenantStatusId
                )
            ))
        ];
    }

    public async Task DeleteConnection(int connectionId)
    {
        var connection =
            await DbLocatorDb
                .Set<ConnectionEntity>()
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId)
            ?? throw new KeyNotFoundException($"Connection with ID {connectionId} not found.");

        DbLocatorDb.Set<ConnectionEntity>().Remove(connection);
        await DbLocatorDb.SaveChangesAsync();
    }
}
