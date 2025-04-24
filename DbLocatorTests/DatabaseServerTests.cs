using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

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

    // [Fact]
    // public async Task AddAndDeleteDatabaseServer()
    // {
    //     var databaseServerIpAddress = TestHelpers.GetRandomIpAddressString();
    //     var databaseServerId = await _dbLocator.AddDatabaseServer(
    //         databaseServerName,
    //         databaseServerIpAddress,
    //         null,
    //         null,
    //         false
    //     );

    //     await _dbLocator.DeleteDatabaseServer(databaseServerId);
    //     var databaseServers = (await _dbLocator.GetDatabaseServers())
    //         .Where(x => x.Name == databaseServerName)
    //         .ToList();

    //     Assert.Empty(databaseServers);
    // }

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

    // [Fact]
    // public async Task CannotDeleteDatabaseServerWithAssociatedDatabases()
    // {
    //     var serverName = TestHelpers.GetRandomString();
    //     var ipAddress = TestHelpers.GetRandomIpAddressString();
    //     var serverId = await _dbLocator.AddDatabaseServer(serverName, ipAddress, null, null, false);

    //     // Add a database to the server
    //     var databaseTypeName = TestHelpers.GetRandomString();
    //     var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);
    //     var databaseName = TestHelpers.GetRandomString();
    //     await _dbLocator.AddDatabase(databaseName, serverId, databaseTypeId, Status.Active);

    //     // Attempt to delete the server
    //     await Assert.ThrowsAsync<InvalidOperationException>(
    //         async () => await _dbLocator.DeleteDatabaseServer(serverId)
    //     );
    // }

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
        await Assert.ThrowsAsync<ValidationException>(
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

    // [Fact]
    // public async Task DeleteNonExistentDatabaseServerThrowsException()
    // {
    //     await Assert.ThrowsAsync<KeyNotFoundException>(
    //         async () => await _dbLocator.DeleteDatabaseServer(-1)
    //     );
    // }
}
