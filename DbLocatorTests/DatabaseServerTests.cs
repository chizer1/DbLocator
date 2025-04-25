using System.ComponentModel.DataAnnotations;
using DbLocator;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseServers;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

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
            databaseServerIpAddress,
            null,
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
    public async Task AddAndDeleteDatabaseServer()
    {
        var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerIpAddress,
            null,
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
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerIpAddress,
            null,
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
    public async Task AddDatabaseServerWithAllProperties()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var hostName = "test-host";
        var fqdn = "test-host.example.com";
        var isLinkedServer = true;

        var serverId = await _dbLocator.AddDatabaseServer(
            serverName,
            ipAddress,
            hostName,
            fqdn,
            isLinkedServer
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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

        var newName = TestHelpers.GetRandomString();
        var newIpAddress = TestHelpers.GetRandomIpAddressString();
        var newHostName = "updated-host";
        var newFqdn = "updated-host.example.com";

        await _dbLocator.UpdateDatabaseServer(
            serverId,
            newName,
            newIpAddress,
            newHostName,
            newFqdn
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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

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
                    "1.1.1.1",
                    "host",
                    "host.example.com"
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
            databaseServerIpAddress,
            null,
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
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

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
            async () => await _dbLocator.AddDatabaseServer("TestServer", "", "", "", false)
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
            "192.168.1.101",
            "name-test-host1",
            "name-test1.example.com",
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateNameTestServer",
                    "192.168.1.102",
                    "name-test-host2",
                    "name-test2.example.com",
                    false
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
            "192.168.1.201",
            "duplicate-host",
            "host-test1.example.com",
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateHostTestServer2",
                    "192.168.1.202",
                    "duplicate-host",
                    "host-test2.example.com",
                    false
                )
        );

        Assert.Contains(
            "Database Server Host Name 'duplicate-host' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateFqdn_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServer = await _dbLocator.AddDatabaseServer(
            "DuplicateFqdnTestServer1",
            "192.168.1.301",
            "fqdn-test-host1",
            "duplicate.example.com",
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateFqdnTestServer2",
                    "192.168.1.302",
                    "fqdn-test-host2",
                    "duplicate.example.com",
                    false
                )
        );

        Assert.Contains(
            "Database Server Fully Qualified Domain Name 'duplicate.example.com' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateIpAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServer = await _dbLocator.AddDatabaseServer(
            "DuplicateIpTestServer1",
            "192.168.1.400",
            "ip-test-host1",
            "ip-test1.example.com",
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    "DuplicateIpTestServer2",
                    "192.168.1.400",
                    "ip-test-host2",
                    "ip-test2.example.com",
                    false
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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

        // Get servers to populate cache
        var servers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(servers, s => s.Id == serverId);

        // Delete server from database to ensure we're getting from cache
        await _dbLocator.DeleteDatabaseServer(serverId);

        // Act
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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

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
        var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

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
            server1Ip,
            null,
            null,
            false
        );

        var server2Name = TestHelpers.GetRandomString();
        var server2Ip = TestHelpers.GetRandomIpAddressString();
        var server2Id = await _dbLocator.AddDatabaseServer(
            server2Name,
            server2Ip,
            null,
            null,
            false
        );

        // Get servers to populate cache
        var servers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(servers, s => s.Id == server1Id);
        Assert.Contains(servers, s => s.Id == server2Id);

        // Delete servers from database to ensure we're getting from cache
        await _dbLocator.DeleteDatabaseServer(server1Id);
        await _dbLocator.DeleteDatabaseServer(server2Id);

        // Act
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
}
