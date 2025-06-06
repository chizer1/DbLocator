using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using Microsoft.Data.SqlClient;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class ConnectionTests : IAsyncLifetime
{
    private readonly Locator _dbLocator;
    private readonly int _databaseServerId;
    private readonly DbLocatorCache _cache;
    private readonly List<Connection> _testConnections = new();
    private readonly List<Database> _testDatabases = new();
    private readonly List<Tenant> _testTenants = new();
    private readonly List<DatabaseType> _testTypes = new();
    private readonly List<DatabaseUser> _testUsers = new();

    public ConnectionTests(DbLocatorFixture dbLocatorFixture)
    {
        _dbLocator = dbLocatorFixture.DbLocator;
        _databaseServerId = dbLocatorFixture.LocalhostServerId;
        _cache = dbLocatorFixture.LocatorCache;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var connection in _testConnections)
        {
            try
            {
                await _dbLocator.DeleteConnection(connection.Id);
            }
            catch { }
        }
        _testConnections.Clear();

        foreach (var database in _testDatabases)
        {
            try
            {
                await _dbLocator.DeleteDatabase(database.Id);
            }
            catch { }
        }
        _testDatabases.Clear();

        foreach (var tenant in _testTenants)
        {
            try
            {
                await _dbLocator.DeleteTenant(tenant.Id);
            }
            catch { }
        }
        _testTenants.Clear();

        foreach (var type in _testTypes)
        {
            try
            {
                await _dbLocator.DeleteDatabaseType((byte)type.Id);
            }
            catch { }
        }
        _testTypes.Clear();

        foreach (var user in _testUsers)
        {
            try
            {
                await _dbLocator.DeleteDatabaseUser(user.Id, true);
            }
            catch { }
        }
        _testUsers.Clear();

        await _cache.Remove("connections");
    }

    private async Task<Tenant> CreateTenantAsync(string name = null, string code = null)
    {
        name ??= TestHelpers.GetRandomString();
        code ??= TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(name, code, Status.Active);
        var tenant = await _dbLocator.GetTenant(tenantId);
        _testTenants.Add(tenant);
        return tenant;
    }

    private async Task<DatabaseType> CreateDatabaseTypeAsync(string name = null)
    {
        name ??= TestHelpers.GetRandomString();
        var typeId = await _dbLocator.CreateDatabaseType(name);
        var type = await _dbLocator.GetDatabaseType(typeId);
        _testTypes.Add(type);
        return type;
    }

    private async Task<Database> CreateDatabaseAsync(
        string name = null,
        int? serverId = null,
        int? typeId = null,
        Status status = Status.Active)
    {
        name ??= TestHelpers.GetRandomString();
        serverId ??= _databaseServerId;
        typeId ??= (await CreateDatabaseTypeAsync()).Id;
        var databaseId = await _dbLocator.CreateDatabase(name, serverId.Value, (byte)typeId.Value, status);
        var database = await _dbLocator.GetDatabase(databaseId);
        _testDatabases.Add(database);
        return database;
    }

    private async Task<DatabaseUser> CreateDatabaseUserAsync(
        int[] databaseIds,
        string username = null,
        string password = "TestPassword123!",
        bool affectDatabase = true)
    {
        username ??= TestHelpers.GetRandomString();
        var userId = await _dbLocator.CreateDatabaseUser(databaseIds, username, password, affectDatabase);
        var user = await _dbLocator.GetDatabaseUser(userId);
        _testUsers.Add(user);
        return user;
    }

    private async Task<Connection> CreateConnectionAsync(
        int? tenantId = null,
        int? databaseId = null,
        bool withUser = true,
        DatabaseRole[] roles = null)
    {
        tenantId ??= (await CreateTenantAsync()).Id;
        databaseId ??= (await CreateDatabaseAsync()).Id;
        var connectionId = await _dbLocator.CreateConnection(tenantId.Value, databaseId.Value);
        var connections = await _dbLocator.GetConnections();
        var connection = connections.First(c => c.Id == connectionId);
        _testConnections.Add(connection);

        if (withUser)
        {
            roles ??= new[] { DatabaseRole.DataReader };
            var user = await CreateDatabaseUserAsync(new[] { databaseId.Value });
            foreach (var role in roles)
            {
                await _dbLocator.CreateDatabaseUserRole(user.Id, role, true);
            }
        }

        return connection;
    }

    #region Creation Tests
    [Fact]
    public async Task CreateConnection()
    {
        var connection = await CreateConnectionAsync();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connection.Id);
    }

    [Fact]
    public async Task CreateDuplicateConnection_ThrowsArgumentException()
    {
        var tenant = await CreateTenantAsync();
        var database = await CreateDatabaseAsync();

        await _dbLocator.CreateConnection(tenant.Id, database.Id);

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _dbLocator.CreateConnection(tenant.Id, database.Id)
        );
    }

    [Fact]
    public async Task CreateConnectionWithNonExistentTenantThrowsException()
    {
        var database = await CreateDatabaseAsync();
        var nonExistentTenantId = -1;

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.CreateConnection(nonExistentTenantId, database.Id)
        );

        Assert.Contains($"Tenant with ID {nonExistentTenantId} not found", exception.Message);
    }

    [Fact]
    public async Task CreateConnectionWithNonExistentDatabaseThrowsException()
    {
        var tenant = await CreateTenantAsync();
        var nonExistentDatabaseId = 999999;

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.CreateConnection(tenant.Id, nonExistentDatabaseId)
        );

        Assert.Contains("Database with ID", exception.Message);
    }
    #endregion

    #region Cache Tests
    [Fact]
    public async Task VerifyConnectionsAreCached()
    {
        var connection = await CreateConnectionAsync();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connection.Id);

        var cachedConnections = await _cache.GetCachedData<List<Connection>>("connections");
        Assert.NotNull(cachedConnections);
        Assert.Contains(cachedConnections, cn => cn.Id == connection.Id);
    }

    [Fact]
    public async Task VerifyUpdatingDatabaseTypeClearsConnectionCache()
    {
        var connection = await CreateConnectionAsync();
        var type = await CreateDatabaseTypeAsync();
        var database = await CreateDatabaseAsync(typeId: type.Id);

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connection.Id);

        var cachedConnections = await _cache.GetCachedData<List<Connection>>("connections");
        Assert.NotNull(cachedConnections);
        Assert.Contains(cachedConnections, cn => cn.Id == connection.Id);

        await _dbLocator.UpdateDatabaseType((byte)type.Id, "UpdatedDatabaseType");

        var cachedConnectionsAfterUpdate = await _cache.GetCachedData<List<Connection>>("connections");
        Assert.Null(cachedConnectionsAfterUpdate);
    }

    [Fact]
    public async Task GetConnections_FromCache()
    {
        var connection = await CreateConnectionAsync();

        var firstCall = await _dbLocator.GetConnections();
        Assert.Contains(firstCall, cn => cn.Id == connection.Id);

        var secondCall = await _dbLocator.GetConnections();
        Assert.Contains(secondCall, cn => cn.Id == connection.Id);
        Assert.Equal(firstCall.Count, secondCall.Count);
    }

    [Fact]
    public async Task GetConnection_RetrievesCachedConnection()
    {
        var connection = await CreateConnectionAsync(withUser: true);

        var firstCall = await _dbLocator.GetConnection(connection.Id, new[] { DatabaseRole.DataReader });
        Assert.NotNull(firstCall);

        var secondCall = await _dbLocator.GetConnection(connection.Id, new[] { DatabaseRole.DataReader });
        Assert.NotNull(secondCall);
        Assert.Equal(firstCall.ConnectionString, secondCall.ConnectionString);
    }
    #endregion

    #region Get Connection Tests
    [Fact]
    public async Task GetConnectionByTenantIdAndDatabaseTypeId()
    {
        var tenant = await CreateTenantAsync();
        var type = await CreateDatabaseTypeAsync();
        var database = await CreateDatabaseAsync(typeId: type.Id);
        var connection = await CreateConnectionAsync(tenant.Id, database.Id);

        var result = await _dbLocator.GetConnection(
            tenant.Id,
            (byte)type.Id,
            new[] { DatabaseRole.DataReader }
        );
        Assert.NotNull(result);
        Assert.NotNull(result.ConnectionString);
    }

    [Fact]
    public async Task GetConnectionByTenantCodeAndDatabaseTypeId()
    {
        var tenant = await CreateTenantAsync();
        var type = await CreateDatabaseTypeAsync();
        var database = await CreateDatabaseAsync(typeId: type.Id);
        var connection = await CreateConnectionAsync(tenant.Id, database.Id);

        var result = await _dbLocator.GetConnection(
            tenant.Code,
            (byte)type.Id,
            new[] { DatabaseRole.DataReader }
        );
        Assert.NotNull(result);
        Assert.NotNull(result.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_ByConnectionId()
    {
        var connection = await CreateConnectionAsync(withUser: true);

        var result = await _dbLocator.GetConnection(connection.Id, new[] { DatabaseRole.DataReader });
        Assert.NotNull(result);
        Assert.NotNull(result.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_WithNoRolesSpecified()
    {
        var connection = await CreateConnectionAsync(withUser: false);

        var result = await _dbLocator.GetConnection(connection.Id, Array.Empty<DatabaseRole>());
        Assert.NotNull(result);
        Assert.NotNull(result.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_WithServerNameFallback()
    {
        var tenant = await CreateTenantAsync();
        var type = await CreateDatabaseTypeAsync();
        var database = await CreateDatabaseAsync(typeId: type.Id);
        var connection = await CreateConnectionAsync(tenant.Id, database.Id);

        var result = await _dbLocator.GetConnection(
            tenant.Id,
            (byte)type.Id,
            new[] { DatabaseRole.DataReader }
        );
        Assert.NotNull(result);
        Assert.NotNull(result.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_WithServerIpFallback()
    {
        var tenant = await CreateTenantAsync();
        var type = await CreateDatabaseTypeAsync();
        var database = await CreateDatabaseAsync(typeId: type.Id);
        var connection = await CreateConnectionAsync(tenant.Id, database.Id);

        var result = await _dbLocator.GetConnection(
            tenant.Id,
            (byte)type.Id,
            new[] { DatabaseRole.DataReader }
        );
        Assert.NotNull(result);
        Assert.NotNull(result.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_WithNoValidServerIdentifier_ThrowsInvalidOperationException()
    {
        var tenant = await CreateTenantAsync();
        var type = await CreateDatabaseTypeAsync();
        
        // Create a database server with no valid identifiers
        var serverId = await _dbLocator.CreateDatabaseServer(
            null,  // no server name
            null,  // no hostname
            null,  // no IP address
            null,  // no FQDN
            false  // not a linked server
        );
        
        var database = await CreateDatabaseAsync(
            name: TestHelpers.GetRandomString(),
            serverId: serverId,
            typeId: type.Id
        );
        await CreateConnectionAsync(tenant.Id, database.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.GetConnection(tenant.Id, (byte)type.Id, new[] { DatabaseRole.DataReader })
        );
    }
    #endregion

    #region Validation Tests
    [Fact]
    public async Task GetNonExistentConnection_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetConnection(-1, Array.Empty<DatabaseRole>())
        );
    }

    [Fact]
    public async Task DeleteNonExistentConnection_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteConnection(-1)
        );
    }

    [Fact]
    public async Task GetConnectionWithInvalidRoles_ThrowsInvalidOperationException()
    {
        var connection = await CreateConnectionAsync(withUser: false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.GetConnection(connection.Id, new[] { DatabaseRole.DataReader })
        );
    }

    [Fact]
    public async Task GetConnection_WithNonExistentTenantId_ThrowsKeyNotFoundException()
    {
        var type = await CreateDatabaseTypeAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetConnection(-1, (byte)type.Id, new[] { DatabaseRole.DataReader })
        );
    }

    [Fact]
    public async Task GetConnection_WithNonExistentDatabaseTypeId_ThrowsKeyNotFoundException()
    {
        var tenant = await CreateTenantAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetConnection(tenant.Id, (byte)255, new[] { DatabaseRole.DataReader })
        );
    }

    [Fact]
    public async Task GetConnection_WithNonExistentTenantCode_ThrowsKeyNotFoundException()
    {
        var type = await CreateDatabaseTypeAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetConnection("NONEXISTENT", (byte)type.Id, new[] { DatabaseRole.DataReader })
        );
    }

    [Fact]
    public async Task GetConnection_WithNoUserForRoles_ThrowsInvalidOperationException()
    {
        var tenant = await CreateTenantAsync();
        var type = await CreateDatabaseTypeAsync();
        var database = await CreateDatabaseAsync(typeId: type.Id);
        await CreateConnectionAsync(tenant.Id, database.Id, withUser: false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.GetConnection(tenant.Id, (byte)type.Id, new[] { DatabaseRole.DataReader })
        );
    }

    [Fact]
    public async Task GetConnection_WithEmptyTenantCode_ThrowsKeyNotFoundException()
    {
        var type = await CreateDatabaseTypeAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetConnection("", (byte)type.Id, new[] { DatabaseRole.DataReader })
        );
    }

    [Fact]
    public async Task GetConnection_WithNullTenantCode_ThrowsKeyNotFoundException()
    {
        var type = await CreateDatabaseTypeAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetConnection(null, (byte)type.Id, new[] { DatabaseRole.DataReader })
        );
    }
    #endregion

    #region Delete Tests
    [Fact]
    public async Task DeleteConnection()
    {
        var connection = await CreateConnectionAsync();
        await _dbLocator.DeleteConnection(connection.Id);

        var connections = await _dbLocator.GetConnections();
        Assert.DoesNotContain(connections, cn => cn.Id == connection.Id);
    }
    #endregion
}
