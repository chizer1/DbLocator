using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class ConnectionTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;
    private readonly int _databaseServerId = dbLocatorFixture.LocalhostServerId;
    private readonly DbLocatorCache _cache = dbLocatorFixture.LocatorCache;

    [Fact]
    public async Task AddConnection()
    {
        // Clean up any existing connections
        var existingConnections = await _dbLocator.GetConnections();
        foreach (var connection in existingConnections)
        {
            await _dbLocator.DeleteConnection(connection.Id);
        }

        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task VerifyConnectionsAreCached()
    {
        // Clean up any existing connections
        var existingConnections = await _dbLocator.GetConnections();
        foreach (var connection in existingConnections)
        {
            await _dbLocator.DeleteConnection(connection.Id);
        }

        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);

        var cachedConnections = await _cache.GetCachedData<List<Connection>>("connections");
        Assert.NotNull(cachedConnections);

        Assert.Contains(cachedConnections, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task VerifyUpdatingDatabaseTypeClearsConnectionCache()
    {
        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);

        var cachedConnections = await _cache.GetCachedData<List<Connection>>("connections");
        Assert.NotNull(cachedConnections);

        Assert.Contains(cachedConnections, cn => cn.Id == connectionId);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        await _dbLocator.UpdateDatabaseType(databaseTypeId, "UpdatedDatabaseType");

        var cachedConnectionsAfterUpdate = await _cache.GetCachedData<List<Connection>>(
            "connections"
        );
        Assert.Null(cachedConnectionsAfterUpdate);
    }

    [Fact]
    public async Task GetConnectionByTenantIdAndDatabaseTypeId()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.AddConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.AddDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            true
        );

        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(connection);
    }

    [Fact]
    public async Task GetConnectionByTenantCodeAndDatabaseTypeId()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.AddConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.AddDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            true
        );

        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            tenantCode,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(connection);
    }

    [Fact]
    public async Task DeleteConnection()
    {
        var connectionId = await GetConnectionId();

        await _dbLocator.DeleteConnection(connectionId);

        var connections = await _dbLocator.GetConnections();
        Assert.DoesNotContain(connections, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task AddDuplicateConnection_ThrowsArgumentException()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        await _dbLocator.AddConnection(tenantId, databaseId);

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _dbLocator.AddConnection(tenantId, databaseId)
        );
    }

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
        var connectionId = await GetConnectionId();
        var dbUserId = await _dbLocator.AddDatabaseUser(
            [connectionId],
            TestHelpers.GetRandomString(),
            true
        );

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.GetConnection(connectionId, [DatabaseRole.DataReader])
        );
    }

    private async Task<int> GetConnectionId()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        return await _dbLocator.AddConnection(tenantId, databaseId);
    }
}
