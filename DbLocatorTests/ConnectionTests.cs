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
    public async Task CreateConnection()
    {
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
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
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
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
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
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
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
    public async Task CreateDuplicateConnection_ThrowsArgumentException()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        await _dbLocator.CreateConnection(tenantId, databaseId);

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _dbLocator.CreateConnection(tenantId, databaseId)
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
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [connectionId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.GetConnection(connectionId, [DatabaseRole.DataReader])
        );
    }

    [Fact]
    public async Task CreateConnectionWithNonExistentTenantThrowsException()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var nonExistentTenantId = -1;
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.CreateConnection(nonExistentTenantId, databaseId)
        );

        Assert.Contains($"Tenant with ID {nonExistentTenantId} not found", exception.Message);
    }

    [Fact]
    public async Task CreateConnectionWithNonExistentDatabaseThrowsException()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var nonExistentDatabaseId = 999999; // Using a positive but non-existent ID
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.CreateConnection(tenantId, nonExistentDatabaseId)
        );

        Assert.Contains("Database with ID", exception.Message);
    }

    [Fact]
    public async Task GetConnectionWithCachedConnectionString()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(connection);
        Assert.NotNull(connection.ConnectionString);
    }

    [Fact]
    public async Task GetConnectionWithTrustedConnection()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            true,
            true
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(connection);
        Assert.NotNull(connection.ConnectionString);
        Assert.Contains("Integrated Security=True", connection.ConnectionString);
    }

    [Fact]
    public async Task GetConnectionWithNonExistentTenantIdThrowsException()
    {
        var nonExistentTenantId = -1;
        var databaseTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.GetConnection(
                    nonExistentTenantId,
                    databaseTypeId,
                    Array.Empty<DatabaseRole>()
                )
        );
    }

    [Fact]
    public async Task GetConnectionWithNonExistentTenantCodeThrowsException()
    {
        var nonExistentTenantCode = "NonExistentCode";
        var databaseTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.GetConnection(
                    nonExistentTenantCode,
                    databaseTypeId,
                    Array.Empty<DatabaseRole>()
                )
        );
    }

    [Fact]
    public async Task GetConnectionWithNonExistentDatabaseTypeThrowsException()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var nonExistentDatabaseTypeId = -1;

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.GetConnection(
                    tenantId,
                    nonExistentDatabaseTypeId,
                    Array.Empty<DatabaseRole>()
                )
        );
    }

    private async Task<int> GetConnectionId()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        return await _dbLocator.CreateConnection(tenantId, databaseId);
    }

    [Fact]
    public async Task GetConnection_WithoutTrustedConnection_ReturnsConnectionWithUserCredentials()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(connection);
        Assert.NotNull(connection.ConnectionString);
        Assert.DoesNotContain("Trusted_Connection=True", connection.ConnectionString);
        Assert.Contains("User ID=", connection.ConnectionString);
        Assert.Contains("Password=", connection.ConnectionString);
    }

    [Fact]
    public async Task GetConnections_FromCache()
    {
        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);

        var cachedConnections = await _cache.GetCachedData<List<Connection>>("connections");
        Assert.NotNull(cachedConnections);
        Assert.Contains(cachedConnections, cn => cn.Id == connectionId);

        var connectionsFromCache = await _dbLocator.GetConnections();
        Assert.Contains(connectionsFromCache, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task GetConnection_RetrievesCachedConnection()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(connection);

        var cachedConnection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(cachedConnection);
        Assert.NotNull(cachedConnection.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_ByConnectionId()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(connectionId);
        Assert.NotNull(connection);
        Assert.NotNull(connection.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_WithNoRolesSpecified()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.CreateConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            "TestPassword123!",
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            Array.Empty<DatabaseRole>()
        );
        Assert.NotNull(connection);
        Assert.NotNull(connection.ConnectionString);
    }

    [Fact]
    public async Task GetConnection_WithNonExistentConnectionId_ThrowsKeyNotFoundException()
    {
        var nonExistentConnectionId = -1;
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetConnection(nonExistentConnectionId)
        );
    }

    [Fact]
    public async Task GetConnection_WithInvalidQueryParameters_ThrowsKeyNotFoundException()
    {
        var nonExistentTenantId = -1;
        var nonExistentDatabaseTypeId = -1;
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.GetConnection(
                    nonExistentTenantId,
                    nonExistentDatabaseTypeId,
                    Array.Empty<DatabaseRole>()
                )
        );
    }

    [Fact]
    public async Task GetConnections_WithNullCache()
    {
        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);

        await _cache.Remove("connections");

        var connectionsAfterCacheRemoval = await _dbLocator.GetConnections();
        Assert.Contains(connectionsAfterCacheRemoval, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task GetConnection_WithNoParameters_ThrowsKeyNotFoundException()
    {
        // Use invalid tenantId and databaseTypeId to simulate 'no parameters'
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dbLocator.GetConnection(-1, -1, null)
        );
    }

    [Fact]
    public async Task GetConnection_WithNonExistentTenantId_ThrowsKeyNotFoundException_Explicit()
    {
        var nonExistentTenantId = -9999;
        var databaseTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dbLocator.GetConnection(nonExistentTenantId, databaseTypeId, null)
        );
    }

    [Fact]
    public async Task GetConnection_WithNonExistentDatabaseTypeId_ThrowsKeyNotFoundException_Explicit()
    {
        var tenantId = await _dbLocator.CreateTenant(TestHelpers.GetRandomString());
        var nonExistentDatabaseTypeId = -9999;
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dbLocator.GetConnection(tenantId, nonExistentDatabaseTypeId, null)
        );
    }

    [Fact]
    public async Task GetConnection_WithNonExistentTenantCode_ThrowsKeyNotFoundException_Explicit()
    {
        var databaseTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _dbLocator.GetConnection("NonExistentCode", databaseTypeId, null)
        );
    }

    [Fact]
    public async Task GetConnection_WithNoValidServerIdentifier_ThrowsInvalidOperationException()
    {
        // Create a database server with all name fields empty
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () =>
        {
            var serverId = await _dbLocator.CreateDatabaseServer("", "", "", "", false);
            var databaseTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());
            var databaseId = await _dbLocator.CreateDatabase(
                TestHelpers.GetRandomString(),
                serverId,
                databaseTypeId,
                Status.Active
            );
            var tenantId = await _dbLocator.CreateTenant(TestHelpers.GetRandomString());
            await _dbLocator.CreateConnection(tenantId, databaseId);
            await _dbLocator.GetConnection(tenantId, databaseTypeId, null);
        });
    }

    [Fact]
    public async Task GetConnection_WithNoUserForRoles_ThrowsInvalidOperationException()
    {
        var tenantId = await _dbLocator.CreateTenant(TestHelpers.GetRandomString());
        var databaseTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());
        var databaseId = await _dbLocator.CreateDatabase(
            TestHelpers.GetRandomString(),
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );
        await _dbLocator.CreateConnection(tenantId, databaseId);
        // Do NOT create any user for this database
        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                _dbLocator.GetConnection(
                    tenantId,
                    databaseTypeId,
                    new[] { DatabaseRole.DataReader }
                )
        );
    }
}
