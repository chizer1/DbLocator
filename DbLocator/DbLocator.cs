using System.Runtime.CompilerServices;
using DbLocator.Db;
using DbLocator.Library;
using DbLocator.Utilities;
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
    private readonly Connections _connections;
    private readonly Databases _databases;
    private readonly DatabaseUsers _databaseUsers;
    private readonly DatabaseUserRoles _databaseUserRoles;
    private readonly DatabaseServers _databaseServers;
    private readonly DatabaseTypes _databaseTypes;
    private readonly Tenants _tenants;

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
                "DbLocator connection string is required.",
                nameof(dbLocatorConnectionString)
            );

        ApplyMigrations(dbLocatorConnectionString);

        var dbContextFactory = DbContextFactory.CreateDbContextFactory(dbLocatorConnectionString);
        var encryption = new Encryption(encryptionKey);

        var dbLocatorCache = new DbLocatorCache(distributedCache);

        _connections = new Connections(dbContextFactory, encryption, dbLocatorCache);
        _databases = new Databases(dbContextFactory, dbLocatorCache);
        _databaseServers = new DatabaseServers(dbContextFactory, dbLocatorCache);
        _databaseUsers = new DatabaseUsers(dbContextFactory, encryption, dbLocatorCache);
        _databaseUserRoles = new DatabaseUserRoles(dbContextFactory);
        _databaseTypes = new DatabaseTypes(dbContextFactory, dbLocatorCache);
        _tenants = new Tenants(dbContextFactory, dbLocatorCache);
    }

    /// <summary>
    /// Applies database migrations to ensure the database schema is up-to-date.
    /// </summary>
    /// <param name="dbLocatorConnectionString">The connection string for the DbLocator database.</param>
    private static void ApplyMigrations(string dbLocatorConnectionString)
    {
        using var dbLocator = new DbLocatorContext(
            new DbContextOptionsBuilder<DbLocatorContext>()
                .UseSqlServer(dbLocatorConnectionString)
                .Options
        );

        dbLocator.Database.Migrate();
    }
}
