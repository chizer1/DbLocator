using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections;

internal record GetConnectionQuery(
    int? TenantId,
    int? DatabaseTypeId,
    int? ConnectionId,
    string TenantCode,
    DatabaseRole[] Roles = null
);

internal sealed class GetConnectionQueryValidator : AbstractValidator<GetConnectionQuery>
{
    internal GetConnectionQueryValidator() { }
}

internal class GetConnection(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encrypytion,
    DbLocatorCache cache
)
{
    public async Task<SqlConnection> Handle(GetConnectionQuery query)
    {
        await new GetConnectionQueryValidator().ValidateAndThrowAsync(query);

        // Override the default toString so I can control it
        var queryString =
            @$"TenantId:{query.TenantId},
            DatabaseTypeId:{query.DatabaseTypeId},
            ConnectionId:{query.ConnectionId},
            TenantCode:{query.TenantCode}
            Roles:{(query.Roles?.Length > 0 ? string.Join(",", query.Roles) : "None")}";

        var cacheKey = $"connection:{queryString}";
        var cachedData = await cache?.GetCachedData<string>(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return new SqlConnection(cachedData);
        }

        var connectionString = await GetConnectionStringFromDatabase(query);
        await cache?.CacheConnectionString(cacheKey, connectionString);

        return new SqlConnection(connectionString);
    }

    private async Task<string> GetConnectionStringFromDatabase(GetConnectionQuery query)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var connectionEntity = await GetConnectionEntityAsync(dbContext, query);
        var database = await GetDatabaseEntityAsync(dbContext, connectionEntity.DatabaseId);

        return await BuildConnectionString(database, encrypytion, dbContext, query.Roles);
    }

    private static async Task<ConnectionEntity> GetConnectionEntityAsync(
        DbLocatorContext dbContext,
        GetConnectionQuery query
    )
    {
        if (query.TenantId.HasValue && query.DatabaseTypeId.HasValue)
        {
            await EnsureTenantExistsAsync(dbContext, query.TenantId.Value);
            await EnsureDatabaseTypeExistsAsync(dbContext, query.DatabaseTypeId.Value);

            return await dbContext
                    .Set<ConnectionEntity>()
                    .Include(con => con.Database)
                    .Where(con =>
                        con.TenantId == query.TenantId
                        && con.Database.DatabaseTypeId == query.DatabaseTypeId
                    )
                    .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException(
                    $"Connection not found with Tenant Id '{query.TenantId}' and Database Type Id '{query.DatabaseTypeId}'."
                );
        }
        else if (query.ConnectionId.HasValue)
        {
            return await dbContext
                    .Set<ConnectionEntity>()
                    .Include(con => con.Database)
                    .FirstOrDefaultAsync(con => con.ConnectionId == query.ConnectionId)
                ?? throw new KeyNotFoundException(
                    $"Connection with ID {query.ConnectionId} not found."
                );
        }
        else if (!string.IsNullOrEmpty(query.TenantCode) && query.DatabaseTypeId.HasValue)
        {
            await EnsureTenantExistsAsync(dbContext, query.TenantCode);
            await EnsureDatabaseTypeExistsAsync(dbContext, query.DatabaseTypeId.Value);

            return await dbContext
                    .Set<ConnectionEntity>()
                    .Include(con => con.Database)
                    .Where(con =>
                        con.Tenant.TenantCode == query.TenantCode
                        && con.Database.DatabaseTypeId == query.DatabaseTypeId
                    )
                    .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException(
                    $"Connection not found with Tenant Code '{query.TenantCode}' and Database Type Id '{query.DatabaseTypeId}'."
                );
        }

        throw new ArgumentException("Invalid query parameters.");
    }

    private static async Task EnsureTenantExistsAsync(DbLocatorContext dbContext, int tenantId)
    {
        var tenantExists = await dbContext
            .Set<TenantEntity>()
            .AnyAsync(t => t.TenantId == tenantId);
        if (!tenantExists)
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");
    }

    private static async Task EnsureTenantExistsAsync(DbLocatorContext dbContext, string tenantCode)
    {
        var tenantExists = await dbContext
            .Set<TenantEntity>()
            .AnyAsync(t => t.TenantCode == tenantCode);
        if (!tenantExists)
            throw new KeyNotFoundException($"Tenant Code '{tenantCode}' not found.");
    }

    private static async Task EnsureDatabaseTypeExistsAsync(
        DbLocatorContext dbContext,
        int databaseTypeId
    )
    {
        var databaseTypeExists = await dbContext
            .Set<DatabaseTypeEntity>()
            .AnyAsync(dt => dt.DatabaseTypeId == databaseTypeId);
        if (!databaseTypeExists)
            throw new KeyNotFoundException($"Database type with ID {databaseTypeId} not found.");
    }

    private static async Task<DatabaseEntity> GetDatabaseEntityAsync(
        DbLocatorContext dbContext,
        int databaseId
    )
    {
        return await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(d => d.DatabaseId == databaseId)
            ?? throw new KeyNotFoundException($"Database with ID {databaseId} not found.");
    }

    private static async Task<string> BuildConnectionString(
        DatabaseEntity database,
        Encryption encrypytion,
        DbLocatorContext dbContext,
        DatabaseRole[] roles = null
    )
    {
        var dataSource =
            database.DatabaseServer.DatabaseServerFullyQualifiedDomainName
            ?? database.DatabaseServer.DatabaseServerIpaddress
            ?? database.DatabaseServer.DatabaseServerHostName;

        var connectionStringBuilder = new SqlConnectionStringBuilder
        {
            DataSource = dataSource,
            InitialCatalog = database.DatabaseName,
            TrustServerCertificate = true
        };

        if (database.UseTrustedConnection)
        {
            connectionStringBuilder.IntegratedSecurity = true;
            return connectionStringBuilder.ConnectionString;
        }

        var user = await GetDatabaseUser(database, dbContext, roles);
        connectionStringBuilder.IntegratedSecurity = false;
        connectionStringBuilder.UserID = user.UserName;
        connectionStringBuilder.Password = encrypytion.Decrypt(user.UserPassword);
        return connectionStringBuilder.ConnectionString;
    }

    private static async Task<DatabaseUserEntity> GetDatabaseUser(
        DatabaseEntity database,
        DbLocatorContext dbContext,
        DatabaseRole[] roleList
    )
    {
        // For trusted connections, we don't need a database user
        if (database.UseTrustedConnection)
        {
            return null;
        }

        var roles =
            roleList != null && roleList.Length > 0
                ? roleList.Select(r => (int)r)
                : [(int)DatabaseRole.DataReader, (int)DatabaseRole.DataWriter];

        var users = await dbContext
            .Set<DatabaseUserEntity>()
            .Include(u => u.UserRoles)
            .Where(u =>
                dbContext
                    .Set<DatabaseUserDatabaseEntity>()
                    .Any(dud =>
                        dud.DatabaseUserId == u.DatabaseUserId
                        && dud.DatabaseId == database.DatabaseId
                    )
            )
            .ToListAsync();

        // Find a user that matches all the roles
        return users.FirstOrDefault(u =>
                !roles.Except(u.UserRoles.Select(ur => ur.DatabaseRoleId)).Any()
            )
            ?? throw new InvalidOperationException(
                $"No suitable database user found for database '{database.DatabaseName}' with roles {(roleList?.Length > 0 ? string.Join(", ", roleList) : "None")}."
            );
    }
}
