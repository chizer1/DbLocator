using System.ComponentModel.DataAnnotations;
using DbLocator;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseServers;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseServerTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;
    private readonly DbLocatorCache _cache = dbLocatorFixture.LocatorCache;
    private readonly string databaseServerName = TestHelpers.GetRandomString();

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.AddDatabaseServer(databaseServerName, "", "", "", false)
        );

        Assert.Contains("At least one of the following fields must be provided", exception.Message);
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateServerName_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingServerName = TestHelpers.GetRandomString();
        await _dbLocator.AddDatabaseServer(existingServerName, "192.168.1.1", "", "", false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(existingServerName, "192.168.1.2", "", "", false)
        );

        Assert.Contains(
            $"Database Server Name '{existingServerName}' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateHostName_ThrowsInvalidOperationException()
    {
        // Arrange
        var duplicateHostName = "duplicate-host";
        await _dbLocator.AddDatabaseServer(
            TestHelpers.GetRandomString(),
            "",
            duplicateHostName,
            "",
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    TestHelpers.GetRandomString(),
                    "",
                    duplicateHostName,
                    "",
                    false
                )
        );

        Assert.Contains(
            $"Database Server Host Name '{duplicateHostName}' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateFqdn_ThrowsInvalidOperationException()
    {
        // Arrange
        var duplicateFqdn = "server1.example.com";
        await _dbLocator.AddDatabaseServer(
            TestHelpers.GetRandomString(),
            "",
            "",
            duplicateFqdn,
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    TestHelpers.GetRandomString(),
                    "",
                    "",
                    duplicateFqdn,
                    false
                )
        );

        Assert.Contains(
            $"Database Server Fully Qualified Domain Name '{duplicateFqdn}' already exists",
            exception.Message
        );
    }

    [Fact]
    public async Task AddDatabaseServer_WithDuplicateIpAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var duplicateIp = "192.168.1.1";
        await _dbLocator.AddDatabaseServer(
            TestHelpers.GetRandomString(),
            duplicateIp,
            "",
            "",
            false
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseServer(
                    TestHelpers.GetRandomString(),
                    duplicateIp,
                    "",
                    "",
                    false
                )
        );

        Assert.Contains(
            $"Database Server IP Address '{duplicateIp}' already exists",
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
}
