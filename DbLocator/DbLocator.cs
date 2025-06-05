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
/// The Locator class serves as the main entry point for interacting with the DbLocator database management system.
/// It provides a comprehensive set of methods and services for managing various aspects of database infrastructure:
///
/// <list type="bullet">
///     <item><description>Tenant Management: Create, update, and manage multi-tenant database environments</description></item>
///     <item><description>Connection Management: Handle database connections with support for encryption of sensitive data</description></item>
///     <item><description>Database Operations: Manage database creation, configuration, and maintenance</description></item>
///     <item><description>Server Management: Configure and monitor database servers</description></item>
///     <item><description>User Management: Handle database user accounts and permissions</description></item>
///     <item><description>Role Management: Manage database user roles and access control</description></item>
///     <item><description>Database Type Management: Support for different database types and configurations</description></item>
/// </list>
///
/// The class implements several key features:
/// <list type="bullet">
///     <item><description>Distributed Caching: Improves performance by caching frequently accessed data using the provided IDistributedCache implementation</description></item>
///     <item><description>Data Encryption: Secures sensitive information using the provided encryption key for all data-at-rest operations</description></item>
///     <item><description>Connection Pooling: Efficiently manages database connections through connection pooling</description></item>
///     <item><description>Automatic Migrations: Ensures database schema is always up-to-date with the latest version</description></item>
/// </list>
///
/// All operations are performed through dedicated services that handle specific aspects of database management.
/// The class is designed to be thread-safe and can be used in multi-threaded environments.
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
    /// Gets a SQL connection instance for direct interaction with the DbLocator database.
    /// This connection is initialized during the constructor and can be used for custom SQL operations
    /// that are not covered by the standard service methods.
    ///
    /// Note: This connection is created during initialization and should be disposed of by the caller
    /// when no longer needed. The connection is not automatically managed by the class.
    /// </summary>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the connection fails.</exception>
    public SqlConnection SqlConnection { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Locator"/> class with the specified configuration.
    /// This constructor sets up all necessary services and connections for database management.
    /// </summary>
    /// <param name="dbLocatorConnectionString">The connection string for the DbLocator database. Must be a valid SQL Server connection string.</param>
    /// <param name="encryptionKey">The encryption key used for encrypting and decrypting sensitive data. If not provided, encryption features will be disabled.</param>
    /// <param name="distributedCache">An optional distributed cache implementation for caching database operations. If provided, improves performance by reducing database load.</param>
    /// <exception cref="ArgumentException">Thrown when the connection string is null, empty, or contains only whitespace.</exception>
    /// <exception cref="SqlException">Thrown when there is an error establishing the database connection or when the connection string is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is an error applying database migrations or when the database schema is incompatible.</exception>
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

        SqlConnection = new SqlConnection(dbLocatorConnectionString);
    }

    /// <summary>
    /// Applies database migrations to ensure the database schema is up-to-date with the latest version.
    /// This method is called during initialization to guarantee that the database structure matches
    /// the current application version.
    /// </summary>
    /// <param name="connectionString">The connection string for the DbLocator database. Must be a valid SQL Server connection string.</param>
    /// <exception cref="ArgumentException">Thrown when the connection string is invalid or malformed.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when the database is not accessible.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is an error applying migrations, such as when the database is in an inconsistent state.</exception>
    private static void ApplyMigrations(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbLocatorContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var dbLocator = new DbLocatorContext(optionsBuilder.Options);
        dbLocator.Database.Migrate();
    }
}
