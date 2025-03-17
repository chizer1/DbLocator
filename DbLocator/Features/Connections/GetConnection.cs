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
    Encryption encrypytion
)
{
    internal async Task<SqlConnection> Handle(GetConnectionQuery query)
    {
        await new GetConnectionQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var connectionEntity = await GetConnectionEntityAsync(dbContext, query);
        var database = await GetDatabaseEntityAsync(dbContext, connectionEntity.DatabaseId);

        var connectionString = await BuildConnectionString(
            database,
            encrypytion,
            dbContext,
            query.Roles
        );

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

    private static async Task<string> BuildConnectionString(
        DatabaseEntity database,
        Encryption encrypytion,
        DbLocatorContext dbContext,
        DatabaseRole[] roles = null
    )
    {
        // try to connect using the fully qualified domain name, if not available try the ip address
        // then try the host name
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
        }
        else
        {
            var user = await GetDatabaseUser(database, dbContext, encrypytion, roles);
            connectionStringBuilder.UserID = user.UserName;
            connectionStringBuilder.Password = encrypytion.Decrypt(user.UserPassword);
        }

        return connectionStringBuilder.ConnectionString;
    }

    private static async Task<DatabaseUserEntity> GetDatabaseUser(
        DatabaseEntity database,
        DbLocatorContext dbContext,
        Encryption encryption,
        DatabaseRole[] roleList = null
    )
    {
        roleList ??= [DatabaseRole.DataReader, DatabaseRole.DataWriter];
        var roles = string.Join('_', roleList.Select(r => ((int)r).ToString()));
        var user = await CreateDatabaseUser(database, dbContext, encryption, roleList, roles);

        return user;
    }

    private static async Task<DatabaseUserEntity> CreateDatabaseUser(
        DatabaseEntity database,
        DbLocatorContext dbContext,
        Encryption encryption,
        DatabaseRole[] roleList,
        string roles
    )
    {
        var user = await dbContext.DatabaseUsers.SingleOrDefaultAsync(u =>
            u.DatabaseId == database.DatabaseId && u.Roles == roles
        );

        if (user != null)
        {
            return user;
        }

        var password = Guid.NewGuid().ToString();
        var username = $"User_{database.DatabaseId}_{database.DatabaseServerId}_{roles}";
        user = new DatabaseUserEntity
        {
            DatabaseId = database.DatabaseId,
            UserName = username,
            Roles = roles,
            UserPassword = encryption.Encrypt(password)
        };

        await dbContext.Set<DatabaseUserEntity>().AddAsync(user);
        await dbContext.SaveChangesAsync();

        var commands = new List<string>
        {
            $"create login {username} with password = '{password}'",
            $"use {database.DatabaseName}; create user {username} for login {user.UserName}"
        };

        foreach (var role in roleList)
        {
            var roleName = Enum.GetName(role).ToLower();
            commands.Add(
                $"use {database.DatabaseName}; exec sp_addrolemember 'db_{roleName}', '{user.UserName}'"
            );
        }

        try
        {
            foreach (var commandText in commands)
            {
                using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = commandText;
                await dbContext.Database.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException)
        {
            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "use [DbLocator]";
            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();

            dbContext.Set<DatabaseUserEntity>().Remove(user);
            await dbContext.SaveChangesAsync();
            throw;
        }

        return user;
    }
}
