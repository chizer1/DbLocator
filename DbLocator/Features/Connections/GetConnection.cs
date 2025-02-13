using System.Data.SqlClient;
using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections;

internal record GetConnectionQuery(
    int? TenantId,
    int? DatabaseTypeId,
    int? ConnectionId,
    string TenantCode
);

internal sealed class GetConnectionQueryValidator : AbstractValidator<GetConnectionQuery>
{
    internal GetConnectionQueryValidator() { }
}

internal class GetConnection(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encrypytion
)
{
    internal async Task<SqlConnection> Handle(GetConnectionQuery query)
    {
        await new GetConnectionQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var connectionEntity = await GetConnectionEntityAsync(dbContext, query);
        var database = await GetDatabaseEntityAsync(dbContext, connectionEntity.DatabaseId);

        var connectionString = BuildConnectionString(database, encrypytion);

        return new SqlConnection(connectionString);
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
                    $"Connection Id '{query.ConnectionId}' not found."
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
            throw new KeyNotFoundException($"Tenant Id '{tenantId}' not found.");
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
            throw new KeyNotFoundException($"Database Type Id '{databaseTypeId}' not found.");
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
            ?? throw new KeyNotFoundException($"Database with Id {databaseId} not found.");
    }

    private static string BuildConnectionString(DatabaseEntity database, Encryption encrypytion)
    {
        return database.UseTrustedConnection
            ? $"Server={database.DatabaseServer.DatabaseServerName};Database={database.DatabaseName};Trusted_Connection=True;"
            : $"Server={database.DatabaseServer.DatabaseServerName};Database={database.DatabaseName};User Id={database.DatabaseUser};Password={encrypytion.Decrypt(database.DatabaseUserPassword)};";
    }
}
