using System.Runtime.CompilerServices;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Library;
using DbLocator.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

[assembly: InternalsVisibleTo("DbLocatorTests")]

namespace DbLocator;

/// <summary>
/// The Locator class provides methods to interact with the DbLocator database.
/// Including operations for tenants, connections, databases, database servers, and database types.
/// </summary>
public class Locator
{
    private readonly Connections _connections;
    private readonly Databases _databases;
    private readonly DatabaseUsers _databaseUsers;
    private readonly DatabaseUserRoles _databaseUserRoles;
    private readonly DatabaseServers _databaseServers;
    private readonly DatabaseTypes _databaseTypes;
    private readonly Tenants _tenants;

    /// <summary>
    /// Initializes a new instance of the <see cref="Locator"/> class with the specified connection string.
    /// This constructor sets up the database context, applies migrations, and initializes the various services.
    /// </summary>
    /// <param name="dbLocatorConnectionString">The connection string for the DbLocator database.</param>
    /// <exception cref="ArgumentException">Thrown when the connection string is null or whitespace.</exception>
    public Locator(string dbLocatorConnectionString)
        : this(dbLocatorConnectionString, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Locator"/> class with the specified connection string and encryption key.
    /// This constructor sets up the database context, applies migrations, and initializes the various services.
    /// </summary>
    /// <param name="dbLocatorConnectionString">The connection string for the DbLocator database.</param>
    /// <param name="encryptionKey">The encryption key for encrypting and decrypting sensitive data.</param>
    /// <exception cref="ArgumentException">Thrown when the connection string is null or whitespace.</exception>
    public Locator(string dbLocatorConnectionString, string encryptionKey)
        : this(dbLocatorConnectionString, encryptionKey, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Locator"/> class with the specified connection string, encryption key, and distributed cache.
    /// This constructor sets up the database context, applies migrations, and initializes the various services.
    /// </summary>
    /// <param name="dbLocatorConnectionString">The connection string for the DbLocator database.</param>
    /// <param name="encryptionKey">The encryption key for encrypting and decrypting sensitive data.</param>
    /// <param name="distributedCache">The distributed cache for caching data.</param>
    /// <exception cref="ArgumentException">Thrown when the connection string is null or whitespace.</exception>
    public Locator(
        string dbLocatorConnectionString,
        string encryptionKey,
        IDistributedCache distributedCache
    )
    {
        if (string.IsNullOrWhiteSpace(dbLocatorConnectionString))
            throw new ArgumentException("DbLocator connection string is required.");

        using (
            var dbLocator = new DbLocatorContext(
                new DbContextOptionsBuilder<DbLocatorContext>()
                    .UseSqlServer(dbLocatorConnectionString)
                    .Options
            )
        )
        {
            dbLocator.Database.Migrate();
        }

        var dbContextFactory = DbContextFactory.CreateDbContextFactory(dbLocatorConnectionString);

        var encryption = new Encryption(encryptionKey);
        _connections = new Connections(dbContextFactory, encryption);
        _databases = new Databases(dbContextFactory);
        _databaseServers = new DatabaseServers(dbContextFactory);
        _databaseUsers = new DatabaseUsers(dbContextFactory, encryption);
        _databaseUserRoles = new DatabaseUserRoles(dbContextFactory);
        _databaseTypes = new DatabaseTypes(dbContextFactory);
        _tenants = new Tenants(dbContextFactory, distributedCache);
    }

    #region Tenants

    /// <summary>
    ///Add tenant
    /// </summary>
    /// <param name="tenantName"></param>
    /// <param name="tenantCode"></param>
    /// <param name="tenantStatus"></param>
    /// <returns>TenantId</returns>
    public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _tenants.AddTenant(tenantName, tenantCode, tenantStatus);
    }

    /// <summary>
    ///Add tenant
    /// </summary>
    /// <param name="tenantName"></param>
    /// <param name="tenantStatus"></param>
    /// <returns>TenantId</returns>
    public async Task<int> AddTenant(string tenantName, Status tenantStatus)
    {
        return await _tenants.AddTenant(tenantName, tenantStatus);
    }

    /// <summary>
    ///Add Tenant
    /// </summary>
    /// <returns>TenantId</returns>
    /// <param name="tenantName"></param>
    public async Task<int> AddTenant(string tenantName)
    {
        return await _tenants.AddTenant(tenantName);
    }

    /// <summary>
    ///Get tenants
    /// </summary>
    /// <returns>List of tenants</returns>
    public async Task<List<Tenant>> GetTenants()
    {
        return await _tenants.GetTenants();
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantName"></param>
    /// <param name="tenantCode"></param>
    /// <param name="tenantStatus"></param>
    /// <returns></returns>
    public async Task UpdateTenant(
        int tenantId,
        string tenantName,
        string tenantCode,
        Status tenantStatus
    )
    {
        await _tenants.UpdateTenant(tenantId, tenantName, tenantCode, tenantStatus);
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantName"></param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, string tenantName)
    {
        await _tenants.UpdateTenant(tenantId, tenantName);
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantStatus"></param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, Status tenantStatus)
    {
        await _tenants.UpdateTenant(tenantId, tenantStatus);
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantName"></param>
    /// <param name="tenantCode"></param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, string tenantName, string tenantCode)
    {
        await _tenants.UpdateTenant(tenantId, tenantName, tenantCode);
    }

    /// <summary>
    ///Delete tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public async Task DeleteTenant(int tenantId)
    {
        await _tenants.DeleteTenant(tenantId);
    }

    #endregion

    #region Connections

    /// <summary>
    /// Get SQL connection
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="roles"></param>
    /// <returns>SqlConnection</returns>
    public async Task<SqlConnection> GetConnection(int connectionId, DatabaseRole[] roles = null)
    {
        return await _connections.GetConnection(connectionId, roles);
    }

    /// <summary>
    /// Get SQL connection
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="roles"></param>
    /// <returns>SqlConnection</returns>
    /// <returns></returns>
    public async Task<SqlConnection> GetConnection(
        int tenantId,
        int databaseTypeId,
        DatabaseRole[] roles = null
    )
    {
        return await _connections.GetConnection(tenantId, databaseTypeId, roles);
    }

    /// <summary>
    /// Get SQL connection
    /// </summary>
    /// <param name="tenantCode"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="roles"></param>
    /// <returns>SqlConnection</returns>
    public async Task<SqlConnection> GetConnection(
        string tenantCode,
        int databaseTypeId,
        DatabaseRole[] roles = null
    )
    {
        return await _connections.GetConnection(tenantCode, databaseTypeId, roles);
    }

    /// <summary>
    /// Get connections
    /// </summary>
    /// <returns>List of connections</returns>
    /// <returns></returns>
    public async Task<List<Connection>> GetConnections()
    {
        return await _connections.GetConnections();
    }

    /// <summary>
    ///Add connection
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="databaseId"></param>
    /// <returns>ConnectionId</returns>
    public async Task<int> AddConnection(int tenantId, int databaseId)
    {
        return await _connections.AddConnection(tenantId, databaseId);
    }

    /// <summary>
    ///Delete connection
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public async Task DeleteConnection(int connectionId)
    {
        await _connections.DeleteConnection(connectionId);
    }

    #endregion

    #region DatabaseUsers

    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <param name="UserPassword"></param>
    /// <param name="CreateUser"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        string UserPassword,
        bool CreateUser
    )
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName, UserPassword, CreateUser);
    }

    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <param name="CreateUser"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(int DatabaseId, string UserName, bool CreateUser)
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName, CreateUser);
    }

    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <param name="UserPassword"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(int DatabaseId, string UserName, string UserPassword)
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName, UserPassword);
    }

    /// <summary>
    /// Add database user
    /// </summary>
    /// <param name="DatabaseId"></param>
    /// <param name="UserName"></param>
    /// <returns>DatabaseUserId</returns>

    public async Task<int> AddDatabaseUser(int DatabaseId, string UserName)
    {
        return await _databaseUsers.AddDatabaseUser(DatabaseId, UserName);
    }

    /// <summary>
    /// Get database users
    /// </summary>
    /// <returns>List of database users</returns>
    public async Task<List<DatabaseUser>> GetDatabaseUsers()
    {
        return await _databaseUsers.GetDatabaseUsers();
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <param name="DatabaseUserPassword"></param>
    /// <param name="UpdateDatabase"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        string DatabaseUserPassword,
        bool UpdateDatabase
    )
    {
        await _databaseUsers.UpdateDatabaseUser(
            DatabaseUserId,
            DatabaseUserName,
            DatabaseUserPassword,
            UpdateDatabase
        );
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <param name="UpdateDatabase"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        bool UpdateDatabase
    )
    {
        await _databaseUsers.UpdateDatabaseUser(DatabaseUserId, DatabaseUserName, UpdateDatabase);
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <param name="DatabaseUserPassword"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        string DatabaseUserPassword
    )
    {
        await _databaseUsers.UpdateDatabaseUser(
            DatabaseUserId,
            DatabaseUserName,
            DatabaseUserPassword
        );
    }

    /// <summary>
    /// Update database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DatabaseUserName"></param>
    /// <returns></returns>

    public async Task UpdateDatabaseUser(int DatabaseUserId, string DatabaseUserName)
    {
        await _databaseUsers.UpdateDatabaseUser(DatabaseUserId, DatabaseUserName);
    }

    /// <summary>
    /// Delete database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="DeleteDatabaseUser"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUser(int DatabaseUserId, bool DeleteDatabaseUser)
    {
        await _databaseUsers.DeleteDatabaseUser(DatabaseUserId, DeleteDatabaseUser);
    }

    /// <summary>
    /// Delete database user
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUser(int DatabaseUserId)
    {
        await _databaseUsers.DeleteDatabaseUser(DatabaseUserId);
    }

    # endregion

    #region DatabaseUserRoles

    /// <summary>
    /// Add database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <param name="UpdateUser"></param>
    /// <returns></returns>
    public async Task AddDatabaseUserRole(
        int DatabaseUserId,
        DatabaseRole UserRole,
        bool UpdateUser
    )
    {
        await _databaseUserRoles.AddDatabaseUserRole(DatabaseUserId, UserRole, UpdateUser);
    }

    /// <summary>
    /// Add database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <returns></returns>
    public async Task AddDatabaseUserRole(int DatabaseUserId, DatabaseRole UserRole)
    {
        await _databaseUserRoles.AddDatabaseUserRole(DatabaseUserId, UserRole);
    }

    /// <summary>
    /// Delete database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <param name="DeleteDatabaseUserRole"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUserRole(
        int DatabaseUserId,
        DatabaseRole UserRole,
        bool DeleteDatabaseUserRole
    )
    {
        await _databaseUserRoles.DeleteDatabaseUserRole(
            DatabaseUserId,
            UserRole,
            DeleteDatabaseUserRole
        );
    }

    /// <summary>
    /// Delete database user role
    /// </summary>
    /// <param name="DatabaseUserId"></param>
    /// <param name="UserRole"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseUserRole(int DatabaseUserId, DatabaseRole UserRole)
    {
        await _databaseUserRoles.DeleteDatabaseUserRole(DatabaseUserId, UserRole);
    }

    #endregion

    #region Databases

    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseStatus"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus
        );
    }

    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseStatus"></param>
    /// <param name="createDatabase"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool createDatabase
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus,
            createDatabase
        );
    }

    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            Status.Active
        );
    }

    /// <summary>
    /// Add database
    /// </summary>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="createDatabase"></param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool createDatabase
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            createDatabase
        );
    }

    /// <summary>
    ///Get databases
    /// </summary>
    /// <returns>List of Databases</returns>
    public async Task<List<Database>> GetDatabases()
    {
        return await _databases.GetDatabases();
    }

    /// <summary>
    ///Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseName"></param>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseStatus"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(
        int databaseId,
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        await _databases.UpdateDatabase(
            databaseId,
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus
        );
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseServerId"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, int databaseServerId)
    {
        await _databases.UpdateDatabase(databaseId, databaseServerId);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseTypeId"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, byte databaseTypeId)
    {
        await _databases.UpdateDatabase(databaseId, databaseTypeId);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, string databaseName)
    {
        await _databases.UpdateDatabase(databaseId, databaseName);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="databaseStatus"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, Status databaseStatus)
    {
        await _databases.UpdateDatabase(databaseId, databaseStatus);
    }

    /// <summary>
    /// Update database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="useTrustedConnection"></param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, bool useTrustedConnection)
    {
        await _databases.UpdateDatabase(databaseId, useTrustedConnection);
    }

    /// <summary>
    ///Delete database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    public async Task DeleteDatabase(int databaseId)
    {
        await _databases.DeleteDatabase(databaseId);
    }

    /// <summary>
    ///Delete database
    /// </summary>
    /// <param name="databaseId"></param>
    /// <param name="deleteDatabase"></param>
    /// <returns></returns>
    public async Task DeleteDatabase(int databaseId, bool deleteDatabase)
    {
        await _databases.DeleteDatabase(databaseId, deleteDatabase);
    }

    #endregion

    #region DatabaseServers

    /// <summary>
    ///Add database server, need to provide at least one of the following fields: Database Server Host Name, Database Server Fully Qualified Domain Name, Database Server IP Address.
    /// </summary>
    /// <param name="databaseServerName"></param>
    /// <param name="databaseServerIpAddress"></param>
    /// <param name="databaseServerHostName"></param>
    /// <param name="databaseServerFullyQualifiedDomainName"></param>
    /// <param name="isLinkedServer"></param>
    /// <returns>DatabaseServerId</returns>
    public async Task<int> AddDatabaseServer(
        string databaseServerName,
        string databaseServerIpAddress,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName,
        bool isLinkedServer
    )
    {
        return await _databaseServers.AddDatabaseServer(
            databaseServerName,
            isLinkedServer,
            databaseServerIpAddress,
            databaseServerHostName,
            databaseServerFullyQualifiedDomainName
        );
    }

    /// <summary>
    ///Get database servers
    /// </summary>
    /// <returns>List of database servers</returns>
    /// <returns></returns>
    public async Task<List<DatabaseServer>> GetDatabaseServers()
    {
        return await _databaseServers.GetDatabaseServers();
    }

    /// <summary>
    ///Update database server, need to provide at least one of the following fields: Database Server Host Name, Database Server Fully Qualified Domain Name, Database Server IP Address.
    /// </summary>
    /// <param name="databaseServerId"></param>
    /// <param name="databaseServerName"></param>
    /// <param name="databaseServerIpAddress"></param>
    /// <param name="databaseServerHostName"></param>
    /// <param name="databaseServerFullyQualifiedDomainName"></param>
    /// <returns></returns>
    public async Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerIpAddress,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName
    )
    {
        await _databaseServers.UpdateDatabaseServer(
            databaseServerId,
            databaseServerName,
            databaseServerIpAddress,
            databaseServerHostName,
            databaseServerFullyQualifiedDomainName
        );
    }

    /// <summary>
    ///Delete database server
    /// </summary>
    /// <param name="databaseServerId"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseServer(int databaseServerId)
    {
        await _databaseServers.DeleteDatabaseServer(databaseServerId);
    }

    #endregion

    #region DatabaseTypes

    /// <summary>
    ///Add database type
    /// </summary>
    /// <param name="databaseTypeName"></param>
    /// <returns>DatabaseTypeId</returns>
    public async Task<byte> AddDatabaseType(string databaseTypeName)
    {
        return await _databaseTypes.AddDatabaseType(databaseTypeName);
    }

    /// <summary>
    ///Get database types
    /// </summary>
    /// <returns>List of database types</returns>
    public async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        return await _databaseTypes.GetDatabaseTypes();
    }

    /// <summary>
    ///Update database type
    /// </summary>
    /// <param name="databaseTypeId"></param>
    /// <param name="databaseTypeName"></param>
    /// <returns></returns>
    public async Task UpdateDatabaseType(byte databaseTypeId, string databaseTypeName)
    {
        await _databaseTypes.UpdateDatabaseType(databaseTypeId, databaseTypeName);
    }

    /// <summary>
    ///Delete database type
    /// </summary>
    /// <param name="databaseTypeId"></param>
    /// <returns></returns>
    public async Task DeleteDatabaseType(byte databaseTypeId)
    {
        await _databaseTypes.DeleteDatabaseType(databaseTypeId);
    }

    #endregion
}
