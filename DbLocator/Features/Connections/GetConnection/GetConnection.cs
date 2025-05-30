#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections.GetConnection;

internal record GetConnectionQuery(
    int? TenantId = null,
    int? DatabaseTypeId = null,
    int? ConnectionId = null,
    string? TenantCode = null,
    DatabaseRole[]? Roles = null
);

internal sealed class GetConnectionQueryValidator : AbstractValidator<GetConnectionQuery>
{
    internal GetConnectionQueryValidator()
    {
        RuleFor(query => query.ConnectionId)
            .NotEmpty()
            .When(query => query.TenantId == null && query.TenantCode == null)
            .WithMessage("Either ConnectionId, TenantId, or TenantCode must be provided.");

        RuleFor(query => query.TenantId)
            .NotEmpty()
            .When(query => query.ConnectionId == null && query.TenantCode == null)
            .WithMessage("Either ConnectionId, TenantId, or TenantCode must be provided.");

        RuleFor(query => query.TenantCode)
            .NotEmpty()
            .When(query => query.ConnectionId == null && query.TenantId == null)
            .WithMessage("Either ConnectionId, TenantId, or TenantCode must be provided.");

        RuleFor(query => query.DatabaseTypeId)
            .NotEmpty()
            .When(query => query.ConnectionId == null)
            .WithMessage("DatabaseTypeId is required when not using ConnectionId.");

        // Roles are optional - removed the NotEmpty validation
    }
}

internal class GetConnectionHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly Encryption _encryption = encryption;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<SqlConnection> Handle(
        GetConnectionQuery request,
        CancellationToken cancellationToken = default
    )
    {
        // Only validate if we have a valid request
        if (request.ConnectionId.HasValue || request.TenantId.HasValue || !string.IsNullOrEmpty(request.TenantCode))
        {
            await new GetConnectionQueryValidator().ValidateAndThrowAsync(request, cancellationToken);
        }
        else
        {
            throw new KeyNotFoundException("No valid connection parameters provided");
        }

        if (_cache != null)
        {
            var rolesString = request.Roles != null ? string.Join(",", request.Roles) : "none";
            var queryString =
                @$"TenantId:{request.TenantId},
                DatabaseTypeId:{request.DatabaseTypeId},
                ConnectionId:{request.ConnectionId},
                TenantCode:{request.TenantCode},
                Roles:{rolesString}";
            var cacheKey = $"connection:{queryString}";

            var cachedConnectionString = await _cache.GetCachedData<string>(cacheKey);
            if (cachedConnectionString != null)
                return new SqlConnection(cachedConnectionString);
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var connection =
            await GetConnectionEntity(request, dbContext, cancellationToken)
            ?? throw new KeyNotFoundException("Connection not found.");
        var connectionString = await GetConnectionString(
            connection,
            request.Roles ?? Array.Empty<DatabaseRole>(),
            _encryption,
            dbContext,
            cancellationToken
        );

        if (_cache != null)
        {
            var rolesString = request.Roles != null ? string.Join(",", request.Roles) : "none";
            var queryString =
                @$"TenantId:{request.TenantId},
                DatabaseTypeId:{request.DatabaseTypeId},
                ConnectionId:{request.ConnectionId},
                TenantCode:{request.TenantCode},
                Roles:{rolesString}";
            var cacheKey = $"connection:{queryString}";

            await _cache.CacheConnectionString(cacheKey, connectionString);
        }

        return new SqlConnection(connectionString);
    }

    private static async Task<ConnectionEntity?> GetConnectionEntity(
        GetConnectionQuery request,
        DbLocatorContext dbContext,
        CancellationToken cancellationToken
    )
    {
        // Validate query parameters
        if (request.ConnectionId.HasValue && request.ConnectionId.Value <= 0)
        {
            throw new KeyNotFoundException($"Connection with ID {request.ConnectionId} not found.");
        }

        if (request.TenantId.HasValue && request.TenantId.Value <= 0)
        {
            throw new KeyNotFoundException($"Tenant with ID {request.TenantId} not found.");
        }

        if (request.DatabaseTypeId.HasValue && request.DatabaseTypeId.Value <= 0)
        {
            throw new KeyNotFoundException($"Database type with ID {request.DatabaseTypeId} not found.");
        }

        // Check if tenant exists
        if (request.TenantId.HasValue)
        {
            var tenantExists = await dbContext
                .Set<TenantEntity>()
                .AnyAsync(t => t.TenantId == request.TenantId, cancellationToken);

            if (!tenantExists)
            {
                throw new KeyNotFoundException($"Tenant with ID {request.TenantId} not found.");
            }
        }

        // Check if tenant code exists
        if (!string.IsNullOrEmpty(request.TenantCode))
        {
            var tenantExists = await dbContext
                .Set<TenantEntity>()
                .AnyAsync(t => t.TenantCode == request.TenantCode, cancellationToken);

            if (!tenantExists)
            {
                throw new KeyNotFoundException($"Tenant with code {request.TenantCode} not found.");
            }
        }

        // Check if database type exists
        if (request.DatabaseTypeId.HasValue)
        {
            var databaseTypeExists = await dbContext
                .Set<DatabaseTypeEntity>()
                .AnyAsync(dt => dt.DatabaseTypeId == request.DatabaseTypeId, cancellationToken);

            if (!databaseTypeExists)
            {
                throw new KeyNotFoundException($"Database type with ID {request.DatabaseTypeId} not found.");
            }
        }

        var queryable = dbContext
            .Set<ConnectionEntity>()
            .Include(c => c.Tenant)
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseType);

        if (request.ConnectionId.HasValue)
        {
            return await queryable.FirstOrDefaultAsync(
                c => c.ConnectionId == request.ConnectionId,
                cancellationToken
            );
        }

        if (request.TenantId.HasValue)
        {
            return await queryable.FirstOrDefaultAsync(
                c =>
                    c.TenantId == request.TenantId
                    && c.Database.DatabaseTypeId == request.DatabaseTypeId,
                cancellationToken
            );
        }

        if (string.IsNullOrEmpty(request.TenantCode))
        {
            throw new KeyNotFoundException($"Tenant with code {request.TenantCode} not found.");
        }
        return await queryable.FirstOrDefaultAsync(
            c =>
                c.Tenant.TenantCode == request.TenantCode
                && c.Database.DatabaseTypeId == request.DatabaseTypeId,
            cancellationToken
        );
    }

    private static async Task<string> GetConnectionString(
        ConnectionEntity connection,
        DatabaseRole[] roles,
        Encryption encryption,
        DbLocatorContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var server = connection.Database.DatabaseServer;
        var database = connection.Database;

        var serverName = server.DatabaseServerFullyQualifiedDomainName;
        if (string.IsNullOrEmpty(serverName))
            serverName = server.DatabaseServerHostName;
        if (string.IsNullOrEmpty(serverName))
            serverName = server.DatabaseServerIpaddress;
        if (string.IsNullOrEmpty(serverName))
            serverName = server.DatabaseServerName;
        if (string.IsNullOrEmpty(serverName))
        {
            throw new InvalidOperationException(
                $"No valid server identifier found for database server {server.DatabaseServerId}"
            );
        }

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = serverName,
            InitialCatalog = database.DatabaseName,
            IntegratedSecurity = database.UseTrustedConnection,
            Encrypt = true,
            TrustServerCertificate = true,
            ConnectTimeout = 30
        };

        // Only try to get a user if roles are specified and not using trusted connection
        if (roles != null && roles.Length > 0 && !database.UseTrustedConnection)
        {
            var user = await GetUserForRoles(connection, roles, dbContext, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException(
                    $"No user found with the specified roles for database {database.DatabaseName}"
                );
            }

            builder.UserID = user.UserName;
            builder.Password = encryption.Decrypt(user.UserPassword);
        }

        return builder.ConnectionString;
    }

    private static async Task<DatabaseUserEntity?> GetUserForRoles(
        ConnectionEntity connection,
        DatabaseRole[] roles,
        DbLocatorContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext
            .Set<DatabaseUserEntity>()
            .Include(du => du.UserRoles)
            .Include(du => du.Databases)
            .Where(du => du.Databases.Any(d => d.DatabaseId == connection.DatabaseId));

        // If roles are specified, filter by roles
        if (roles != null && roles.Length > 0)
        {
            query = query.Where(du => du.UserRoles.Any(ur => roles.Contains((DatabaseRole)ur.DatabaseRoleId)));
        }

        var user = await query.FirstOrDefaultAsync(cancellationToken);
        if (user == null && roles != null && roles.Length > 0)
        {
            throw new InvalidOperationException(
                $"No user found with the specified roles for database {connection.Database.DatabaseName}"
            );
        }
        return user;
    }
}
