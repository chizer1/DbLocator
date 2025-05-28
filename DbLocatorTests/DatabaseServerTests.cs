using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseServerTests : IAsyncLifetime
{
    private readonly Locator _dbLocator;
    private readonly DbLocatorCache _cache;
    private readonly string databaseServerName = TestHelpers.GetRandomString();

    public DatabaseServerTests(DbLocatorFixture dbLocatorFixture)
    {
        _dbLocator = dbLocatorFixture.DbLocator;
        _cache = dbLocatorFixture.LocatorCache;
    }

    public async Task InitializeAsync()
    {
        await _cache.Remove("databaseServers");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddMultipleDatabaseServersAndSearchByKeyWord()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        await _dbLocator.AddDatabaseServer(
            databaseServerName,
            false,
            null,
            databaseServerIpAddress,
            null
        );

        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(databaseServerName, databaseServers[0].Name);
    }

    [Fact]
    public async Task AddAndDeleteDatabaseServer()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            false,
            null,
            databaseServerIpAddress,
            null
        );

        await _dbLocator.DeleteDatabaseServer(databaseServerId);
        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.Empty(databaseServers);
    }

    [Fact]
    public async Task VerifyDatabaseServersAreCached()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            false,
            null,
            databaseServerIpAddress,
            null
        );

        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(databaseServerName, databaseServers[0].Name);

        var cachedDatabaseServers = await _cache.GetCachedData<List<DatabaseServer>>(
            "databaseServers"
        );
        Assert.NotNull(cachedDatabaseServers);
        Assert.Contains(cachedDatabaseServers, ds => ds.Id == databaseServerId);
    }

    [Fact]
    public async Task AddDatabaseServerWithAllProperties()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var hostName = "test-host";
        var fqdn = "test-host.example.com";
        var isLinkedServer = true;

        var serverId = await _dbLocator.AddDatabaseServer(
            serverName,
            isLinkedServer,
            hostName,
            ipAddress,
            fqdn
        );

        var server = (await _dbLocator.GetDatabaseServers()).Single(s => s.Id == serverId);

        Assert.Equal(serverName, server.Name);
        Assert.Equal(ipAddress, server.IpAddress);
        Assert.Equal(hostName, server.HostName);
        Assert.Equal(fqdn, server.FullyQualifiedDomainName);
        Assert.Equal(isLinkedServer, server.IsLinkedServer);
    }

    [Fact]
    public async Task UpdateDatabaseServerProperties()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        var newName = TestHelpers.GetRandomString();
        var newIpAddress = TestHelpers.GetRandomIpAddressString();
        var newHostName = "updated-host";
        var newFqdn = "updated-host.example.com";

        await _dbLocator.UpdateDatabaseServer(
            serverId,
            newName,
            newHostName,
            newFqdn,
            newIpAddress,
            false
        );

        var updatedServer = (await _dbLocator.GetDatabaseServers()).Single(s => s.Id == serverId);

        Assert.Equal(newName, updatedServer.Name);
        Assert.Equal(newIpAddress, updatedServer.IpAddress);
        Assert.Equal(newHostName, updatedServer.HostName);
        Assert.Equal(newFqdn, updatedServer.FullyQualifiedDomainName);
    }

    [Fact]
    public async Task CannotDeleteDatabaseServerWithAssociatedDatabases()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Add a database to the server
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        await _dbLocator.AddDatabase(databaseName, serverId, databaseTypeId, Status.Active);

        // Attempt to delete the server
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseServer(serverId)
        );
    }

    [Fact]
    public async Task GetDatabaseServerById()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        var server = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(server);
        Assert.Equal(serverId, server.Id);
        Assert.Equal(serverName, server.Name);
        Assert.Equal(ipAddress, server.IpAddress);
    }

    [Fact]
    public async Task GetNonExistentDatabaseServerThrowsException()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.GetDatabaseServer(-1)
        );
    }

    [Fact]
    public async Task UpdateNonExistentDatabaseServerThrowsException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(
                    -1,
                    "new-name",
                    "host",
                    "host.example.com",
                    "1.1.1.1",
                    false
                )
        );
    }

    [Fact]
    public async Task DeleteNonExistentDatabaseServerThrowsException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseServer(-1)
        );
    }

    [Fact]
    public async Task DeleteDatabaseServerClearsCache()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            false,
            null,
            databaseServerIpAddress,
            null
        );

        // Ensure cache is populated by getting the servers
        var databaseServers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(databaseServers, ds => ds.Id == databaseServerId);

        // Verify server is in cache
        var cachedDatabaseServers = await _cache.GetCachedData<List<DatabaseServer>>(
            "databaseServers"
        );
        Assert.NotNull(cachedDatabaseServers);
        Assert.Contains(cachedDatabaseServers, ds => ds.Id == databaseServerId);

        // Delete the server
        await _dbLocator.DeleteDatabaseServer(databaseServerId);

        // Verify cache is cleared
        var updatedCache = await _cache.GetCachedData<List<DatabaseServer>>("databaseServers");
        Assert.Null(updatedCache);
    }

    [Fact]
    public async Task CannotDeleteDatabaseServerWithActiveConnections()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Add a database to the server
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            serverId,
            databaseTypeId,
            Status.Active
        );

        // Add a tenant and create a connection to the database
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);
        await _dbLocator.AddConnection(tenantId, databaseId);

        // Attempt to delete the server
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseServer(serverId)
        );
    }

    [Fact]
    public async Task CanDeleteDatabaseServerAfterRemovingAllDatabases()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Add a database to the server
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            serverId,
            databaseTypeId,
            Status.Active
        );

        // Delete the database first
        await _dbLocator.DeleteDatabase(databaseId);

        // Now we should be able to delete the server
        await _dbLocator.DeleteDatabaseServer(serverId);

        // Verify server is deleted
        var servers = await _dbLocator.GetDatabaseServers();
        Assert.DoesNotContain(servers, s => s.Id == serverId);
    }

    [Fact]
    public async Task AddDatabaseServer_WithNoHostNameFqdnOrIp_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.AddDatabaseServer("TestServer", false, null, "", null)
        );

        Assert.Contains(
            "At least one of Host Name, FQDN, or IP Address must be provided",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateServerName_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServer = await _dbLocator.AddDatabaseServer(
            "DuplicateNameTestServer",
            false,
            null,
            "192.168.1.101",
            null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateNameTestServer",
                    false,
                    null,
                    "192.168.1.102",
                    null
                )
        );

        Assert.Contains(
            "Database Server Name 'DuplicateNameTestServer' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateHostName_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServer = await _dbLocator.AddDatabaseServer(
            "DuplicateHostTestServer1",
            false,
            null,
            "192.168.1.201",
            null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateHostTestServer2",
                    false,
                    null,
                    "192.168.1.202",
                    null
                )
        );

        Assert.Contains(
            "Database Server Host Name 'DuplicateHostTestServer1' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateFqdn_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServer = await _dbLocator.AddDatabaseServer(
            "DuplicateFqdnTestServer1",
            false,
            null,
            "192.168.1.301",
            null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateFqdnTestServer2",
                    false,
                    null,
                    "192.168.1.302",
                    null
                )
        );

        Assert.Contains(
            "Database Server Fully Qualified Domain Name 'DuplicateFqdnTestServer1' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateIpAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServer = await _dbLocator.AddDatabaseServer(
            "DuplicateIpTestServer1",
            false,
            null,
            "192.168.1.400",
            null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateIpTestServer2",
                    false,
                    null,
                    "192.168.1.400",
                    null
                )
        );

        Assert.Contains(
            "Database Server IP Address '192.168.1.400' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task GetDatabaseServer_ReturnsCachedData()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Get server to populate cache
        var server = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(server);

        // Delete server from database to ensure we're getting from cache
        await _dbLocator.DeleteDatabaseServer(serverId);

        // Act
        var cachedServer = await _dbLocator.GetDatabaseServer(serverId);

        // Assert
        Assert.NotNull(cachedServer);
        Assert.Equal(serverId, cachedServer.Id);
        Assert.Equal(serverName, cachedServer.Name);
        Assert.Equal(ipAddress, cachedServer.IpAddress);
    }

    [Fact]
    public async Task GetDatabaseServers_ReturnsCachedData()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Get servers to populate cache
        var servers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(servers, s => s.Id == serverId);

        // Act - Get servers again (should come from cache)
        var cachedServers = await _dbLocator.GetDatabaseServers();

        // Assert
        Assert.NotNull(cachedServers);
        Assert.Contains(cachedServers, s => s.Id == serverId);
        Assert.Contains(cachedServers, s => s.Name == serverName);
        Assert.Contains(cachedServers, s => s.IpAddress == ipAddress);
    }

    [Fact]
    public async Task GetDatabaseServers_WithEmptyCache_ReturnsFromDatabase()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Clear cache
        await _cache.Remove("databaseServers");

        // Act
        var servers = await _dbLocator.GetDatabaseServers();

        // Assert
        Assert.NotNull(servers);
        Assert.Contains(servers, s => s.Id == serverId);
        Assert.Contains(servers, s => s.Name == serverName);
        Assert.Contains(servers, s => s.IpAddress == ipAddress);

        // Verify cache was populated
        var cachedServers = await _cache.GetCachedData<List<DatabaseServer>>("databaseServers");
        Assert.NotNull(cachedServers);
        Assert.Contains(cachedServers, s => s.Id == serverId);
    }

    [Fact]
    public async Task GetDatabaseServers_WithNullCache_ReturnsFromDatabase()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Set cache to null
        await _cache.CacheData("databaseServers", null);

        // Act
        var servers = await _dbLocator.GetDatabaseServers();

        // Assert
        Assert.NotNull(servers);
        Assert.Contains(servers, s => s.Id == serverId);
        Assert.Contains(servers, s => s.Name == serverName);
        Assert.Contains(servers, s => s.IpAddress == ipAddress);

        // Verify cache was populated
        var cachedServers = await _cache.GetCachedData<List<DatabaseServer>>("databaseServers");
        Assert.NotNull(cachedServers);
        Assert.Contains(cachedServers, s => s.Id == serverId);
    }

    [Fact]
    public async Task GetDatabaseServers_WithMultipleServers_ReturnsAllFromCache()
    {
        // Arrange
        var server1Name = TestHelpers.GetRandomString();
        var server1Ip = TestHelpers.GetRandomIpAddressString();
        var server1Id = await _dbLocator.AddDatabaseServer(
            server1Name,
            false,
            null,
            server1Ip,
            null
        );

        var server2Name = TestHelpers.GetRandomString();
        var server2Ip = TestHelpers.GetRandomIpAddressString();
        var server2Id = await _dbLocator.AddDatabaseServer(
            server2Name,
            false,
            null,
            server2Ip,
            null
        );

        // Get servers to populate cache
        var servers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(servers, s => s.Id == server1Id);
        Assert.Contains(servers, s => s.Id == server2Id);

        // Act - Get servers again (should come from cache)
        var cachedServers = await _dbLocator.GetDatabaseServers();

        // Assert
        Assert.NotNull(cachedServers);
        Assert.Contains(
            cachedServers,
            s => s.Id == server1Id && s.Name == server1Name && s.IpAddress == server1Ip
        );
        Assert.Contains(
            cachedServers,
            s => s.Id == server2Id && s.Name == server2Name && s.IpAddress == server2Ip
        );
    }

    [Fact]
    public async Task AddDatabaseServer_NoValidParameters()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.AddDatabaseServer(serverName, false, null, null, null)
        );

        Assert.Contains(
            "At least one of Host Name, FQDN, or IP Address must be provided",
            exception.Message
        );
    }

    [Fact]
    public async Task UpdateDatabaseServer_NoValidParameters()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, false, null, ipAddress, null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(serverId, null, null, null, null, false)
        );

        Assert.Contains(
            "At least one of Host Name, FQDN, or IP Address must be provided",
            exception.Message
        );
    }
}
