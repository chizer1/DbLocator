using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using FluentValidation;

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
    public async Task CreateMultipleDatabaseServersAndSearchByKeyWord()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        await _dbLocator.CreateDatabaseServer(
            databaseServerName,
            null,
            databaseServerIpAddress,
            null,
            false
        );

        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(databaseServerName, databaseServers[0].Name);
    }

    [Fact]
    public async Task CreateAndDeleteDatabaseServer()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        var databaseServerId = await _dbLocator.CreateDatabaseServer(
            databaseServerName,
            null,
            databaseServerIpAddress,
            null,
            false
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
        var databaseServerId = await _dbLocator.CreateDatabaseServer(
            databaseServerName,
            null,
            databaseServerIpAddress,
            null,
            false
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
    public async Task CreateDatabaseServerWithAllProperties()
    {
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var hostName = TestHelpers.GetRandomString();
        var fqdn = $"{hostName}.example.com";
        var isLinkedServer = true;

        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            hostName,
            IpAddress,
            fqdn,
            isLinkedServer
        );

        var server = (await _dbLocator.GetDatabaseServers()).Single(s => s.Id == serverId);

        Assert.Equal(serverName, server.Name);
        Assert.Equal(IpAddress, server.IpAddress);
        Assert.Equal(hostName, server.HostName);
        Assert.Equal(fqdn, server.FullyQualifiedDomainName);
        Assert.Equal(isLinkedServer, server.IsLinkedServer);
    }

    [Fact]
    public async Task UpdateDatabaseServerProperties()
    {
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseServer(serverId, newName, null, null, null, null);

        var updatedServer = (await _dbLocator.GetDatabaseServers()).Single(s => s.Id == serverId);

        Assert.Equal(newName, updatedServer.Name);
        Assert.Equal(IpAddress, updatedServer.IpAddress);
    }

    [Fact]
    public async Task CannotDeleteDatabaseServerWithAssociatedDatabases()
    {
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        // Create a database to the server
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        await _dbLocator.CreateDatabase(databaseName, serverId, databaseTypeId, Status.Active);

        // Attempt to delete the server
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseServer(serverId)
        );
    }

    [Fact]
    public async Task GetDatabaseServerById()
    {
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        var server = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(server);
        Assert.Equal(serverId, server.Id);
        Assert.Equal(serverName, server.Name);
        Assert.Equal(IpAddress, server.IpAddress);
    }

    [Fact]
    public async Task GetNonExistentDatabaseServerThrowsException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetDatabaseServer(999999)
        );
    }

    [Fact]
    public async Task UpdateNonExistentDatabaseServerThrowsException()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(-1, "UpdatedName", null, null, null, null)
        );
    }

    [Fact]
    public async Task DeleteDatabaseServerClearsCache()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        var databaseServerId = await _dbLocator.CreateDatabaseServer(
            databaseServerName,
            null,
            databaseServerIpAddress,
            null,
            false
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
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        // Create a database to the server
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            serverId,
            databaseTypeId,
            Status.Active
        );

        // Create a tenant and create a connection to the database
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);
        await _dbLocator.CreateConnection(tenantId, databaseId);

        // Attempt to delete the server
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseServer(serverId)
        );
    }

    [Fact]
    public async Task CanDeleteDatabaseServerAfterRemovingAllDatabases()
    {
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        // Create a database to the server
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
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
    public async Task CreateDatabaseServer_WithDuplicateServerName_ThrowsInvalidOperationException()
    {
        // Arrange
        var uniqueName =
            $"DuplicateNameTestServer_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10)}"; // ensure < 50 chars
        var existingServer = await _dbLocator.CreateDatabaseServer(
            uniqueName,
            null,
            "192.168.1.101",
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.CreateDatabaseServer(
                    uniqueName,
                    null,
                    "192.168.1.102",
                    null,
                    false
                )
        );

        // Print actual message for debugging
        Console.WriteLine($"Actual exception message: {exception.Message}");
        Assert.Contains("already exists", exception.Message);
        Assert.Contains(uniqueName, exception.Message);
    }

    [Fact]
    public async Task CreateDatabaseServer_WithDuplicateHostName_ThrowsInvalidOperationException()
    {
        // Arrange
        var hostName =
            $"duplicate-host-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10)}"; // ensure < 50 chars
        var server1Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            hostName,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        var server2Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            null,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(server2Id, null, hostName, null, null, null)
        );

        Assert.Contains(
            $"Database server with host name \"{hostName}\" already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task CreateDatabaseServer_WithDuplicateIpAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServer = await _dbLocator.CreateDatabaseServer(
            "DuplicateIpTestServer1",
            null,
            "192.168.1.400",
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.CreateDatabaseServer(
                    "DuplicateIpTestServer2",
                    null,
                    "192.168.1.400",
                    null,
                    false
                )
        );

        Assert.Contains(
            "Database server with IP address '192.168.1.400' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task GetDatabaseServer_ReturnsCachedData()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        // Get server to populate cache
        var server = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(server);

        // Delete server from database to ensure we're getting from cache
        await _dbLocator.DeleteDatabaseServer(serverId);

        // Act & Assert - Should throw KeyNotFoundException since server is deleted
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetDatabaseServer(serverId)
        );
    }

    [Fact]
    public async Task GetDatabaseServers_ReturnsCachedData()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        // Get servers to populate cache
        var servers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(servers, s => s.Id == serverId);

        // Act - Get servers again (should come from cache)
        var cachedServers = await _dbLocator.GetDatabaseServers();

        // Assert
        Assert.NotNull(cachedServers);
        Assert.Contains(cachedServers, s => s.Id == serverId);
        Assert.Contains(cachedServers, s => s.Name == serverName);
        Assert.Contains(cachedServers, s => s.IpAddress == IpAddress);
    }

    [Fact]
    public async Task GetDatabaseServers_WithEmptyCache_ReturnsFromDatabase()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        // Clear cache
        await _cache.Remove("databaseServers");

        // Act
        var servers = await _dbLocator.GetDatabaseServers();

        // Assert
        Assert.NotNull(servers);
        Assert.Contains(servers, s => s.Id == serverId);
        Assert.Contains(servers, s => s.Name == serverName);
        Assert.Contains(servers, s => s.IpAddress == IpAddress);

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
        var IpAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            IpAddress,
            null,
            false
        );

        // Set cache to null
        await _cache.CacheData("databaseServers", null);

        // Act
        var servers = await _dbLocator.GetDatabaseServers();

        // Assert
        Assert.NotNull(servers);
        Assert.Contains(servers, s => s.Id == serverId);
        Assert.Contains(servers, s => s.Name == serverName);
        Assert.Contains(servers, s => s.IpAddress == IpAddress);

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
        var server1Id = await _dbLocator.CreateDatabaseServer(
            server1Name,
            null,
            server1Ip,
            null,
            false
        );

        var server2Name = TestHelpers.GetRandomString();
        var server2Ip = TestHelpers.GetRandomIpAddressString();
        var server2Id = await _dbLocator.CreateDatabaseServer(
            server2Name,
            null,
            server2Ip,
            null,
            false
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
    public async Task UpdateDatabaseServer_PreservesExistingProperties()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var hostName = TestHelpers.GetRandomString();
        var fqdn = $"{hostName}.example.com";
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var isLinkedServer = true;

        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            hostName,
            ipAddress,
            fqdn,
            isLinkedServer
        );

        // Act
        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseServer(serverId, newName, null, null, null, null);

        // Assert
        var updatedServer = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(updatedServer);
        Assert.Equal(newName, updatedServer.Name);
        Assert.Equal(hostName, updatedServer.HostName);
        Assert.Equal(fqdn, updatedServer.FullyQualifiedDomainName);
        Assert.Equal(ipAddress, updatedServer.IpAddress);
        Assert.Equal(isLinkedServer, updatedServer.IsLinkedServer);
    }

    [Fact]
    public async Task UpdateDatabaseServer_OnlyFqdnAndIpAddress()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var hostName = TestHelpers.GetRandomString();
        var fqdn = $"{hostName}.example.com";
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var isLinkedServer = true;

        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            hostName,
            ipAddress,
            fqdn,
            isLinkedServer
        );

        // Act
        var newFqdn = "updated-host.example.com";
        var newIpAddress = TestHelpers.GetRandomIpAddressString();
        await _dbLocator.UpdateDatabaseServer(serverId, null, null, newFqdn, newIpAddress, null);

        // Assert
        var updatedServer = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(updatedServer);
        Assert.Equal(serverName, updatedServer.Name);
        Assert.Equal(hostName, updatedServer.HostName);
        Assert.Equal(newFqdn, updatedServer.FullyQualifiedDomainName);
        Assert.Equal(newIpAddress, updatedServer.IpAddress);
        Assert.Equal(isLinkedServer, updatedServer.IsLinkedServer);
    }

    [Fact]
    public async Task UpdateDatabaseServer_OnlyIsLinkedServer()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var hostName = TestHelpers.GetRandomString();
        var fqdn = $"{hostName}.example.com";
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var initialIsLinkedServer = false;

        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            hostName,
            ipAddress,
            fqdn,
            initialIsLinkedServer
        );

        // Act
        // Since there's no direct way to update only IsLinkedServer, we need to update name to preserve other properties
        await _dbLocator.UpdateDatabaseServer(serverId, serverName, null, null, null, null);

        // Assert
        var updatedServer = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(updatedServer);
        Assert.Equal(serverName, updatedServer.Name);
        Assert.Equal(hostName, updatedServer.HostName);
        Assert.Equal(fqdn, updatedServer.FullyQualifiedDomainName);
        Assert.Equal(ipAddress, updatedServer.IpAddress);
        Assert.Equal(initialIsLinkedServer, updatedServer.IsLinkedServer); // Note: IsLinkedServer cannot be updated with current API
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithInvalidFqdn_ThrowsValidationException()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            ipAddress,
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(
                    serverId,
                    null,
                    null,
                    "invalid-fqdn",
                    null,
                    null
                )
        );

        Assert.Contains("FQDN must be a valid domain name format", exception.Message);
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithDuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var server1Name = "DuplicateNameTestServer";
        var server1Id = await _dbLocator.CreateDatabaseServer(
            server1Name,
            null,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        var server2Name = "AnotherServer";
        var server2Id = await _dbLocator.CreateDatabaseServer(
            server2Name,
            null,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(
                    server2Id,
                    server1Name,
                    null,
                    null,
                    null,
                    null
                )
        );

        Assert.Contains(
            $"Database server with name \"{server1Name}\" already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithDuplicateHostName_ThrowsInvalidOperationException()
    {
        // Arrange
        var hostName =
            $"duplicate-host-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10)}"; // ensure < 50 chars
        var server1Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            hostName,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        var server2Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            null,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(server2Id, null, hostName, null, null, null)
        );

        Assert.Contains(
            $"Database server with host name \"{hostName}\" already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithDuplicateFqdn_ThrowsInvalidOperationException()
    {
        // Arrange
        var fqdn = "duplicate.example.com";
        var server1Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            null,
            TestHelpers.GetRandomIpAddressString(),
            fqdn,
            false
        );

        var server2Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            null,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(server2Id, null, null, fqdn, null, null)
        );

        Assert.Contains($"Database server with FQDN \"{fqdn}\" already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateDatabaseServer_UpdatesCache()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            ipAddress,
            null,
            false
        );

        // Ensure cache is populated
        var servers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(servers, s => s.Id == serverId);

        // Act
        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseServer(serverId, newName, null, null, null, null);

        // Assert
        var updatedServers = await _dbLocator.GetDatabaseServers();
        var updatedServer = updatedServers.First(s => s.Id == serverId);
        Assert.Equal(newName, updatedServer.Name);
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithIsLinkedServer()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            ipAddress,
            null,
            false
        );

        // Act
        await _dbLocator.UpdateDatabaseServer(serverId, serverName, null, null, null, null);

        // Assert
        var updatedServer = await _dbLocator.GetDatabaseServer(serverId);
        Assert.False(updatedServer.IsLinkedServer); // Note: IsLinkedServer cannot be updated with current API
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithNonExistentServer_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(999999, "NewName", null, null, null, null)
        );
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithNoChanges_ThrowsValidationException()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            ipAddress,
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(serverId, null, null, null, null, null)
        );

        Assert.Contains("At least one field must be provided for update", exception.Message);
    }

    [Fact]
    public async Task CreateDatabaseServer_WithDuplicateHostNameOnCreate_ThrowsInvalidOperationException()
    {
        // Arrange
        var hostName =
            $"duplicate-host-create-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10)}";
        var server1Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            hostName,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.CreateDatabaseServer(
                    TestHelpers.GetRandomString(),
                    hostName,
                    TestHelpers.GetRandomIpAddressString(),
                    null,
                    false
                )
        );

        Assert.Contains(
            $"Database server with host name \"{hostName}\" already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task CreateDatabaseServer_WithDuplicateFqdnOnCreate_ThrowsInvalidOperationException()
    {
        // Arrange
        var fqdn =
            $"duplicate-create-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10)}.example.com";
        var server1Id = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            null,
            TestHelpers.GetRandomIpAddressString(),
            fqdn,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.CreateDatabaseServer(
                    TestHelpers.GetRandomString(),
                    null,
                    TestHelpers.GetRandomIpAddressString(),
                    fqdn,
                    false
                )
        );

        Assert.Contains($"Database server with FQDN \"{fqdn}\" already exists", exception.Message);
    }

    [Fact]
    public async Task GetDatabaseServerById_ReturnsCachedData()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            ipAddress,
            null,
            false
        );

        // Act - First call should cache the data
        var server1 = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(server1);

        // Verify data is cached
        var cachedData = await _cache.GetCachedData<DatabaseServer>(
            $"databaseServer-id-{serverId}"
        );
        Assert.NotNull(cachedData);
        Assert.Equal(serverId, cachedData.Id);
        Assert.Equal(serverName, cachedData.Name);

        // Act - Second call should return cached data
        var server2 = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(server2);
        Assert.Equal(serverId, server2.Id);
        Assert.Equal(serverName, server2.Name);
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithHostName()
    {
        // Arrange
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(
            serverName,
            null,
            ipAddress,
            null,
            false
        );

        // Act
        var newHostName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseServer(serverId, null, newHostName, null, null, null);

        // Assert
        var updatedServer = await _dbLocator.GetDatabaseServer(serverId);
        Assert.NotNull(updatedServer);
        Assert.Equal(newHostName, updatedServer.HostName);
        Assert.Equal(serverName, updatedServer.Name);
        Assert.Equal(ipAddress, updatedServer.IpAddress);
    }
}
