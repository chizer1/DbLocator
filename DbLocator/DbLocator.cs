using System.Runtime.CompilerServices;
using DbLocator.Db;
using DbLocator.Services.Connection;
using DbLocator.Services.Database;
using DbLocator.Services.DatabaseServer;
using DbLocator.Services.DatabaseType;
using DbLocator.Services.DatabaseUser;
using DbLocator.Services.DatabaseUserRole;
using DbLocator.Services.Tenant;
using DbLocator.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

[assembly: InternalsVisibleTo("DbLocatorTests")]

namespace DbLocator;

/// <summary>
/// The Locator class provides methods to interact with the DbLocator database.
/// Including operations for tenants, connections, databases, database servers, database users, database user roles, and database types.
/// </summary>
public partial class Locator
{
    private readonly IConnectionService _connectionService;
    private readonly IDatabaseService _databaseService;
    private readonly IDatabaseUserService _databaseUserService;
    private readonly IDatabaseUserRoleService _databaseUserRoleService;
    private readonly IDatabaseServerService _databaseServerService;
    private readonly IDatabaseTypeService _databaseTypeService;
    private readonly ITenantService _tenantService;

    /// <summary>
    /// Get a sql connection for the DbLocator database.
    /// </summary>
    public SqlConnection SqlConnection { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Locator"/> class.
    /// </summary>
    /// <param name="dbLocatorConnectionString">The connection string for the DbLocator database.</param>
    /// <param name="encryptionKey">The encryption key for encrypting and decrypting sensitive data (optional).</param>
    /// <param name="distributedCache">The distributed cache for caching data (optional).</param>
    /// <exception cref="ArgumentException">Thrown when the connection string is null or whitespace.</exception>
    public Locator(
        string dbLocatorConnectionString,
        string encryptionKey = null,
        IDistributedCache distributedCache = null
    )
    {
        if (string.IsNullOrWhiteSpace(dbLocatorConnectionString))
            throw new ArgumentException(
                "Connection string is required",
                nameof(dbLocatorConnectionString)
            );

        ApplyMigrations(dbLocatorConnectionString);

        var dbContextFactory = DbContextFactory.CreateDbContextFactory(dbLocatorConnectionString);
        var encryption = new Encryption(encryptionKey);
        var dbLocatorCache = new DbLocatorCache(distributedCache);

        _connectionService = new ConnectionService(dbContextFactory, dbLocatorCache, encryption);
        _databaseService = new DatabaseService(dbContextFactory, dbLocatorCache);
        _databaseServerService = new DatabaseServerService(dbContextFactory, dbLocatorCache);
        _databaseTypeService = new DatabaseTypeService(dbContextFactory, dbLocatorCache);
        _databaseUserService = new DatabaseUserService(
            dbContextFactory,
            encryption,
            dbLocatorCache
        );
        _databaseUserRoleService = new DatabaseUserRoleService(dbContextFactory, dbLocatorCache);
        _tenantService = new TenantService(dbContextFactory, dbLocatorCache);
    }

    /// <summary>
    /// Applies database migrations to ensure the database schema is up-to-date.
    /// </summary>
    /// <param name="dbLocatorConnectionString">The connection string for the DbLocator database.</param>
    private static void ApplyMigrations(string connectionString, string provider)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbLocatorContext>();
        
        // Configure the context based on the provider
        switch (provider.ToLowerInvariant())
        {
            case "postgresql":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case "sqlserver":
            default:
                optionsBuilder.UseSqlServer(connectionString);
                break;
        }

        using var dbLocator = new DbLocatorContext(optionsBuilder.Options);
        dbLocator.Database.Migrate();
    }
}
