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
    private readonly string _databaseServerName = TestHelpers.GetRandomString();

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

    private async Task<DatabaseServer> CreateDatabaseServerAsync(
        string name = null,
        string hostName = null,
        string ipAddress = null,
        string fqdn = null,
        bool isLinkedServer = false
    )
    {
        name ??= TestHelpers.GetRandomString();
        ipAddress ??= TestHelpers.GetRandomIpAddressString();
        var serverId = await _dbLocator.CreateDatabaseServer(name, hostName, ipAddress, fqdn, isLinkedServer);
        return (await _dbLocator.GetDatabaseServers()).Single(s => s.Id == serverId);
    }

    #region Creation Tests
    [Fact]
    public async Task CreateMultipleDatabaseServersAndSearchByKeyWord()
    {
        var server = await CreateDatabaseServerAsync(_databaseServerName);

        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == _databaseServerName)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(_databaseServerName, databaseServers[0].Name);
    }

    [Fact]
    public async Task CreateDatabaseServerWithAllProperties()
    {
        var serverName = TestHelpers.GetRandomString();
        var ipAddress = TestHelpers.GetRandomIpAddressString();
        var hostName = TestHelpers.GetRandomString();
        var fqdn = $"{hostName}.example.com";
        var isLinkedServer = true;

        var server = await CreateDatabaseServerAsync(serverName, hostName, ipAddress, fqdn, isLinkedServer);

        Assert.Equal(serverName, server.Name);
        Assert.Equal(ipAddress, server.IpAddress);
        Assert.Equal(hostName, server.HostName);
        Assert.Equal(fqdn, server.FullyQualifiedDomainName);
        Assert.Equal(isLinkedServer, server.IsLinkedServer);
    }

    [Theory]
    [InlineData("DuplicateName", null, null, null)]
    [InlineData(null, "DuplicateHost", null, null)]
    [InlineData(null, null, "192.168.1.1", null)]
    [InlineData(null, null, null, "duplicate.example.com")]
    public async Task CreateDatabaseServer_WithDuplicateProperties_ThrowsInvalidOperationException(
        string name,
        string hostName,
        string ipAddress,
        string fqdn
    )
    {
        // Create first server
        await CreateDatabaseServerAsync(name, hostName, ipAddress, fqdn);

        // Attempt to create second server with same properties
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await CreateDatabaseServerAsync(name, hostName, ipAddress, fqdn)
        );
    }
    #endregion

    #region Cache Tests
    [Fact]
    public async Task VerifyDatabaseServersAreCached()
    {
        var server = await CreateDatabaseServerAsync();

        var databaseServers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(databaseServers, ds => ds.Id == server.Id);

        var cachedDatabaseServers = await _cache.GetCachedData<List<DatabaseServer>>("databaseServers");
        Assert.NotNull(cachedDatabaseServers);
        Assert.Contains(cachedDatabaseServers, ds => ds.Id == server.Id);
    }

    [Fact]
    public async Task GetDatabaseServerById_ReturnsCachedData()
    {
        var server = await CreateDatabaseServerAsync();

        // First call should populate cache
        var firstCall = await _dbLocator.GetDatabaseServer(server.Id);
        Assert.NotNull(firstCall);

        // Second call should use cache
        var secondCall = await _dbLocator.GetDatabaseServer(server.Id);
        Assert.NotNull(secondCall);
        Assert.Equal(firstCall.Id, secondCall.Id);
    }

    [Fact]
    public async Task DeleteDatabaseServerClearsCache()
    {
        var server = await CreateDatabaseServerAsync();

        // Ensure cache is populated
        var databaseServers = await _dbLocator.GetDatabaseServers();
        Assert.Contains(databaseServers, ds => ds.Id == server.Id);

        // Delete the server
        await _dbLocator.DeleteDatabaseServer(server.Id);

        // Verify cache is cleared
        var updatedCache = await _cache.GetCachedData<List<DatabaseServer>>("databaseServers");
        Assert.Null(updatedCache);
    }
    #endregion

    #region Update Tests
    [Fact]
    public async Task UpdateDatabaseServerProperties()
    {
        var server = await CreateDatabaseServerAsync();
        var newName = TestHelpers.GetRandomString();

        await _dbLocator.UpdateDatabaseServer(server.Id, newName, null, null, null, null);

        var updatedServer = await _dbLocator.GetDatabaseServer(server.Id);
        Assert.Equal(newName, updatedServer.Name);
        Assert.Equal(server.IpAddress, updatedServer.IpAddress);
    }

    [Fact]
    public async Task UpdateDatabaseServer_PreservesExistingProperties()
    {
        var server = await CreateDatabaseServerAsync();
        var originalName = server.Name;
        var originalIpAddress = server.IpAddress;

        await _dbLocator.UpdateDatabaseServer(server.Id, null, null, null, null, null);

        var updatedServer = await _dbLocator.GetDatabaseServer(server.Id);
        Assert.Equal(originalName, updatedServer.Name);
        Assert.Equal(originalIpAddress, updatedServer.IpAddress);
    }

    [Fact]
    public async Task UpdateDatabaseServer_WithNoChanges_ThrowsValidationException()
    {
        var server = await CreateDatabaseServerAsync();

        await Assert.ThrowsAsync<ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(
                    server.Id,
                    server.Name,
                    server.HostName,
                    server.IpAddress,
                    server.FullyQualifiedDomainName,
                    server.IsLinkedServer
                )
        );
    }

    [Theory]
    [InlineData("DuplicateName", null, null, null)]
    [InlineData(null, "DuplicateHost", null, null)]
    [InlineData(null, null, "192.168.1.1", null)]
    [InlineData(null, null, null, "duplicate.example.com")]
    public async Task UpdateDatabaseServer_WithDuplicateProperties_ThrowsInvalidOperationException(
        string name,
        string hostName,
        string ipAddress,
        string fqdn
    )
    {
        // Create two servers
        var server1 = await CreateDatabaseServerAsync(name, hostName, ipAddress, fqdn);
        var server2 = await CreateDatabaseServerAsync();

        // Attempt to update second server with first server's properties
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(
                    server2.Id,
                    name,
                    hostName,
                    ipAddress,
                    fqdn,
                    null
                )
        );
    }
    #endregion

    #region Delete Tests
    [Fact]
    public async Task CreateAndDeleteDatabaseServer()
    {
        var server = await CreateDatabaseServerAsync();
        await _dbLocator.DeleteDatabaseServer(server.Id);

        var databaseServers = await _dbLocator.GetDatabaseServers();
        Assert.DoesNotContain(databaseServers, ds => ds.Id == server.Id);
    }

    [Fact]
    public async Task CannotDeleteDatabaseServerWithAssociatedDatabases()
    {
        var server = await CreateDatabaseServerAsync();

        // Create a database on the server
        var databaseTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());
        await _dbLocator.CreateDatabase(
            TestHelpers.GetRandomString(),
            server.Id,
            databaseTypeId,
            Status.Active
        );

        // Attempt to delete the server
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseServer(server.Id)
        );
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
        await Assert.ThrowsAsync<ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseServer(-1, "UpdatedName", null, null, null, null)
        );
    }
    #endregion
}
