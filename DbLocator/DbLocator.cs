using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Library;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("DbLocatorTests")]

namespace DbLocator;

/// <summary>
/// Locator class is the main class that is used to interact with the DbLocator database.
/// </summary>
public class Locator
{
    private readonly Connections _connections;
    private readonly Databases _databases;
    private readonly DatabaseServers _databaseServers;
    private readonly DatabaseTypes _databaseTypes;
    private readonly Tenants _tenants;

    /// <summary>
    /// Constructor for Locator class
    /// </summary>
    public Locator(string dbLocatorConnectionString)
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

        _connections = new Connections(dbContextFactory);
        _databases = new Databases(dbContextFactory);
        _databaseServers = new DatabaseServers(dbContextFactory);
        _databaseTypes = new DatabaseTypes(dbContextFactory);
        _tenants = new Tenants(dbContextFactory);
    }

    #region Tenants

    /// <summary>
    ///Add Tenant to DbLocator database
    /// </summary>
    /// <returns>TenantId</returns>
    /// <param name="tenantName">Name of the Tenant</param>
    /// <param name="tenantCode">Code of the Tenant</param>
    /// <param name="tenantStatus">Status of the Tenant</param>
    public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _tenants.AddTenant(tenantName, tenantCode, tenantStatus);
    }

    /// <summary>
    ///Add Tenant to DbLocator database
    /// </summary>
    /// <returns>TenantId</returns>
    /// <param name="tenantName">Name of the Tenant</param>
    /// <param name="tenantStatus">Status of the Tenant</param>
    public async Task<int> AddTenant(string tenantName, Status tenantStatus)
    {
        return await _tenants.AddTenant(tenantName, tenantStatus);
    }

    /// <summary>
    ///Add Tenant to DbLocator database
    /// </summary>
    /// <returns>TenantId</returns>
    /// <param name="tenantName">Name of the Tenant</param>
    public async Task<int> AddTenant(string tenantName)
    {
        return await _tenants.AddTenant(tenantName);
    }

    /// <summary>
    ///Get Tenants
    /// </summary>
    /// <returns>List of Tenants</returns>
    public async Task<List<Tenant>> GetTenants()
    {
        return await _tenants.GetTenants();
    }

    /// <summary>
    ///Update Tenant information in DbLocator database
    /// </summary>
    /// <param name="tenantId">Id of the Tenant</param>
    /// <param name="tenantName">Name of the Tenant</param>
    /// <param name="tenantCode">Code of the Tenant</param>
    /// <param name="tenantStatus">Status of the Tenant</param>
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
    ///Update Tenant information in DbLocator database
    /// </summary>
    /// <param name="tenantId">Id of the Tenant</param>
    /// <param name="tenantName">Name of the Tenant</param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, string tenantName)
    {
        await _tenants.UpdateTenant(tenantId, tenantName);
    }

    /// <summary>
    ///Update Tenant information in DbLocator database
    /// </summary>
    /// <param name="tenantId">Id of the Tenant</param>
    /// <param name="tenantStatus">Status of the Tenant</param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, Status tenantStatus)
    {
        await _tenants.UpdateTenant(tenantId, tenantStatus);
    }

    /// <summary>
    ///Update Tenant information in DbLocator database
    /// </summary>
    /// <param name="tenantId">Id of the Tenant</param>
    /// <param name="tenantName">Name of the Tenant</param>
    /// <param name="tenantCode">Code of the Tenant</param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, string tenantName, string tenantCode)
    {
        await _tenants.UpdateTenant(tenantId, tenantName, tenantCode);
    }

    /// <summary>
    ///Delete Tenant from DbLocator database
    /// </summary>
    /// <param name="tenantId">Id of the Tenant</param>
    /// <returns></returns>
    public async Task DeleteTenant(int tenantId)
    {
        await _tenants.DeleteTenant(tenantId);
    }

    #endregion

    #region Connections

    /// <summary>
    /// Create SQL connection based on connectionId
    /// </summary>
    /// <param name="connectionId">Id of the connection</param>
    /// <returns>SqlConnection</returns>
    public async Task<SqlConnection> GetConnection(int connectionId)
    {
        return await _connections.GetConnection(connectionId);
    }

    /// <summary>
    /// Create SQL connection based on user, Tenant and database type
    /// </summary>
    /// <param name="tenantId">Id of the Tenant</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <returns>SqlConnection</returns>
    /// <returns></returns>
    public async Task<SqlConnection> GetConnection(int tenantId, int databaseTypeId)
    {
        return await _connections.GetConnection(tenantId, databaseTypeId);
    }

    /// <summary>
    /// Create SQL connection based on tenant code and database type
    /// </summary>
    /// <param name="tenantCode">Code of the Tenant</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <returns>SqlConnection</returns>
    public async Task<SqlConnection> GetConnection(string tenantCode, int databaseTypeId)
    {
        return await _connections.GetConnection(tenantCode, databaseTypeId);
    }

    /// <summary>
    /// Get connections from DbLocator database
    /// </summary>
    /// <returns>List of Connections</returns>
    /// <returns></returns>
    public async Task<List<Connection>> GetConnections()
    {
        return await _connections.GetConnections();
    }

    /// <summary>
    ///Add connection to DbLocator database
    /// </summary>
    /// <param name="TenantUserId">Id of the Tenant user</param>
    /// <param name="databaseId">Id of the database</param>
    /// <returns>ConnectionId</returns>
    /// <returns></returns>
    public async Task<int> AddConnection(int TenantUserId, int databaseId)
    {
        return await _connections.AddConnection(TenantUserId, databaseId);
    }

    /// <summary>
    ///Delete connection from DbLocator database
    /// </summary>
    /// <param name="connectionId">Id of the connection</param>
    /// <returns></returns>
    public async Task DeleteConnection(int connectionId)
    {
        await _connections.DeleteConnection(connectionId);
    }

    #endregion

    #region Databases

    /// <summary>
    /// Insert database record and create new database on specified server with the user provided
    /// </summary>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="databaseUser">User of the database</param>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <param name="databaseStatus">Status of the database</param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        string databaseUser,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseUser,
            databaseServerId,
            databaseTypeId,
            databaseStatus
        );
    }

    /// <summary>
    /// Insert database record and create new database on specified server if createDatabase is true and use trusted connection
    /// </summary>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <param name="databaseStatus">Status of the database</param>
    /// <param name="createDatabase">Create database</param>
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
    /// Insert database record and create new database on specified server with the user provided
    /// </summary>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="databaseUser">User of the database</param>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        string databaseUser,
        int databaseServerId,
        byte databaseTypeId
    )
    {
        return await _databases.AddDatabase(
            databaseName,
            databaseUser,
            databaseServerId,
            databaseTypeId,
            Status.Active
        );
    }

    /// <summary>
    /// Insert database record and create new database on specified server and use trusted connection
    /// </summary>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <returns>DatabaseId</returns>
    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId
    )
    {
        return await _databases.AddDatabase(databaseName, databaseServerId, databaseTypeId);
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
    ///Update database information in DbLocator database and make updates on database server
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="databaseUser">User of the database</param>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <param name="databaseStatus">Status of the database</param>
    /// <returns></returns>
    public async Task UpdateDatabase(
        int databaseId,
        string databaseName,
        string databaseUser,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        await _databases.UpdateDatabase(
            databaseId,
            databaseName,
            databaseUser,
            databaseServerId,
            databaseTypeId,
            databaseStatus
        );
    }

    /// <summary>
    /// Update database information in DbLocator database and make updates on database server
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, int databaseServerId)
    {
        await _databases.UpdateDatabase(databaseId, databaseServerId);
    }

    /// <summary>
    /// Update database information in DbLocator database and make updates on database server
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, byte databaseTypeId)
    {
        await _databases.UpdateDatabase(databaseId, databaseTypeId);
    }

    /// <summary>
    /// Update database information in DbLocator database and make updates on database server
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <param name="databaseName">Name of the database</param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, string databaseName)
    {
        await _databases.UpdateDatabase(databaseId, databaseName);
    }

    /// <summary>
    /// Update database information in DbLocator database and make updates on database server
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="databaseUser">User of the database</param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, string databaseName, string databaseUser)
    {
        await _databases.UpdateDatabase(databaseId, databaseName, databaseUser);
    }

    /// <summary>
    /// Update database information in DbLocator database and make updates on database server
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <param name="databaseStatus">Status of the database</param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, Status databaseStatus)
    {
        await _databases.UpdateDatabase(databaseId, databaseStatus);
    }

    /// <summary>
    /// Update database information in DbLocator database and make updates on database server
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <param name="useTrustedConnection">Use trusted connection</param>
    /// <returns></returns>
    public async Task UpdateDatabase(int databaseId, bool useTrustedConnection)
    {
        await _databases.UpdateDatabase(databaseId, useTrustedConnection);
    }

    /// <summary>
    ///Delete database information from DbLocator database and on server it lives on
    /// </summary>
    /// <param name="databaseId">Id of the database</param>
    /// <returns></returns>
    public async Task DeleteDatabase(int databaseId)
    {
        await _databases.DeleteDatabase(databaseId);
    }

    #endregion

    #region DatabaseServers

    /// <summary>
    ///Add database server to DbLocator database.
    /// </summary>
    /// <param name="databaseServerName">Name of the database server</param>
    /// <param name="databaseServerIpAddress">IP address of the database server</param>
    /// <returns>DatabaseServerId</returns>
    /// <remarks> This method will not create a new database server. It will only add a record in the DbLocator database.</remarks>
    public async Task<int> AddDatabaseServer(
        string databaseServerName,
        string databaseServerIpAddress
    )
    {
        return await _databaseServers.AddDatabaseServer(
            databaseServerName,
            databaseServerIpAddress
        );
    }

    /// <summary>
    ///Get database servers from DbLocator database
    /// </summary>
    /// <returns>List of DatabaseServers</returns>
    /// <returns></returns>
    public async Task<List<DatabaseServer>> GetDatabaseServers()
    {
        return await _databaseServers.GetDatabaseServers();
    }

    /// <summary>
    ///Update database server information in DbLocator database
    /// </summary>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <param name="databaseServerName">Name of the database server</param>
    /// <param name="databaseServerIpAddress">IP address of the database server</param>
    /// <returns></returns>
    public async Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerIpAddress
    )
    {
        await _databaseServers.UpdateDatabaseServer(
            databaseServerId,
            databaseServerName,
            databaseServerIpAddress
        );
    }

    /// <summary>
    ///Delete database server information from DbLocator database
    /// </summary>
    /// <param name="databaseServerId">Id of the database server</param>
    /// <returns></returns>
    public async Task DeleteDatabaseServer(int databaseServerId)
    {
        await _databaseServers.DeleteDatabaseServer(databaseServerId);
    }

    #endregion

    #region DatabaseTypes

    /// <summary>
    ///Add database type to DbLocator database
    /// </summary>
    /// <param name="name">Name of the database type</param>
    /// <returns>DatabaseTypeId</returns>
    public async Task<byte> AddDatabaseType(string name)
    {
        return await _databaseTypes.AddDatabaseType(name);
    }

    /// <summary>
    ///Get database types from DbLocator database
    /// </summary>
    /// <returns>List of DatabaseTypes</returns>
    public async Task<List<DatabaseType>> GetDatabaseTypes()
    {
        return await _databaseTypes.GetDatabaseTypes();
    }

    /// <summary>
    ///Update database type information in DbLocator database
    /// </summary>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <param name="name">Name of the database type</param>
    /// <returns></returns>
    public async Task UpdateDatabaseType(byte databaseTypeId, string name)
    {
        await _databaseTypes.UpdateDatabaseType(databaseTypeId, name);
    }

    /// <summary>
    ///Delete database type from DbLocator database
    /// </summary>
    /// <param name="databaseTypeId">Id of the database type</param>
    /// <returns></returns>
    public async Task DeleteDatabaseType(byte databaseTypeId)
    {
        await _databaseTypes.DeleteDatabaseType(databaseTypeId);
    }

    #endregion
}
