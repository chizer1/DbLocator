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

    [Fact]
    public async Task AddConnectionWithNonExistentTenantThrowsException()
    {
        // Create a database
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        // Try to add connection with non-existent tenant
        var nonExistentTenantId = -1;
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.AddConnection(nonExistentTenantId, databaseId)
        );

        Assert.Contains($"Tenant with ID {nonExistentTenantId} not found", exception.Message);
    }

    [Fact]
    public async Task AddConnectionWithNonExistentDatabaseThrowsException()
    {
        // Create a tenant
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        // Try to add connection with non-existent database
        var nonExistentDatabaseId = -1;
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.AddConnection(tenantId, nonExistentDatabaseId)
        );

        Assert.Contains($"Database with ID {nonExistentDatabaseId} not found", exception.Message);
    }

    [Fact]
    public async Task GetConnectionWithCachedConnectionString()
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

        // First call to populate cache
        var firstConnection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(firstConnection);

        // Clear the cache to ensure we're testing the caching mechanism
        var queryString =
            @$"TenantId:{tenantId},
            DatabaseTypeId:{databaseTypeId},
            ConnectionId:,
            TenantCode:
            Roles:DataReader";
        var cacheKey = $"connection:{queryString}";
        _cache?.Remove(cacheKey);

        // Second call should use cached connection string
        var secondConnection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader]
        );
        Assert.NotNull(secondConnection);
    }

    [Fact]
    public async Task GetConnectionWithTrustedConnection()
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
            Status.Active,
            true
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

    // [Fact]
    // public async Task GetConnectionWithInvalidQueryParametersThrowsException()
    // {
    //     // Create a valid tenant and database type
    //     var tenantName = TestHelpers.GetRandomString();
    //     var tenantId = await _dbLocator.AddTenant(tenantName);
    //     var databaseTypeName = TestHelpers.GetRandomString();
    //     var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

    //     // Try to get connection with a non-existent database type ID
    //     await Assert.ThrowsAsync<ArgumentException>(
    //         async () => await _dbLocator.GetConnection(tenantId, 255, Array.Empty<DatabaseRole>())
    //     );
    // }

    [Fact]
    public async Task GetConnectionWithNonExistentTenantIdThrowsException()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.GetConnection(-1, databaseTypeId, Array.Empty<DatabaseRole>())
        );
    }

    [Fact]
    public async Task GetConnectionWithNonExistentTenantCodeThrowsException()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.GetConnection(
                    "NonExistentCode",
                    databaseTypeId,
                    Array.Empty<DatabaseRole>()
                )
        );
    }

    [Fact]
    public async Task GetConnectionWithNonExistentDatabaseTypeThrowsException()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        // Create a database type and then delete it to ensure it doesn't exist
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
        await _dbLocator.DeleteDatabaseType(databaseTypeId);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.GetConnection(
                    tenantId,
                    databaseTypeId,
                    Array.Empty<DatabaseRole>()
                )
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

    [Fact]
    public async Task GetConnection_WithTrustedConnection_ReturnsConnectionWithIntegratedSecurity()
    {
        // Arrange
        // Create a database with trusted connection enabled
        var databaseName = TestHelpers.GetRandomString();
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active,
            true // Create the database
        );

        // Add a tenant and create a connection
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);
        await _dbLocator.AddConnection(tenantId, databaseId);

        // Enable trusted connection
        await _dbLocator.UpdateDatabase(databaseId, true);

        // Act
        var connection = await _dbLocator.GetConnection(tenantId, databaseTypeId);

        // Assert
        Assert.NotNull(connection);
        var connectionString = connection.ConnectionString;
        Assert.Contains("Integrated Security=True", connectionString);
        Assert.DoesNotContain("User ID=", connectionString);
        Assert.DoesNotContain("Password=", connectionString);
    }

    [Fact]
    public async Task GetConnection_WithoutTrustedConnection_ReturnsConnectionWithUserCredentials()
    {
        // Arrange
        // Create a database with trusted connection disabled
        var databaseName = TestHelpers.GetRandomString();
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active,
            true // Create the database
        );

        // Add a tenant and create a connection
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);
        await _dbLocator.AddConnection(tenantId, databaseId);

        // Disable trusted connection
        await _dbLocator.UpdateDatabase(databaseId, false);

        // Add a database user with required roles
        var dbUserId = await _dbLocator.AddDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            true
        );
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);

        // Act
        var connection = await _dbLocator.GetConnection(tenantId, databaseTypeId);

        // Assert
        Assert.NotNull(connection);
        var connectionString = connection.ConnectionString;
        Assert.Contains("Integrated Security=False", connectionString);
        Assert.Contains("User ID=", connectionString);
        Assert.Contains("Password=", connectionString);
    }

    // create test to get connections from cache
    [Fact]
    public async Task GetConnections_FromCache()
    {
        // Arrange
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

        // Act
        var connections = await _dbLocator.GetConnections();

        // Assert
        Assert.Contains(connections, cn => cn.Id == connectionId);

        // Act again to hit the cache
        var cachedConnections = await _dbLocator.GetConnections();

        // Assert
        Assert.Contains(cachedConnections, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task GetConnection_RetrievesCachedConnection()
    {
        // Arrange
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
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);

        // Act - First call to populate cache
        var firstConnection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader, DatabaseRole.DataWriter]
        );

        // Act - Second call should use cached connection string
        var secondConnection = await _dbLocator.GetConnection(
            tenantId,
            databaseTypeId,
            [DatabaseRole.DataReader, DatabaseRole.DataWriter]
        );

        // Assert
        Assert.NotNull(secondConnection);
        Assert.Equal(firstConnection.ConnectionString, secondConnection.ConnectionString);

        // Verify the connection string was cached
        var queryString =
            @$"TenantId:{tenantId},
            DatabaseTypeId:{databaseTypeId},
            ConnectionId:,
            TenantCode:,
            Roles:DataReader,DataWriter";
        var cacheKey = $"connection:{queryString}";
        var cachedConnectionString = await _cache.GetCachedData<string>(cacheKey);
        Assert.NotNull(cachedConnectionString);
        Assert.Equal(secondConnection.ConnectionString, cachedConnectionString);
    }
}
