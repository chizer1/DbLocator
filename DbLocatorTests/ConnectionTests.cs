using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using Microsoft.Extensions.Caching.Distributed;

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
        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task VerifyConnectionsAreCached()
    {
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
