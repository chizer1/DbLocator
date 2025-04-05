using DbLocator;
using DbLocator.Domain;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class ConnectionTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;
    private readonly int _databaseServerId = dbLocatorFixture.LocalhostServerId;

    [Fact]
    public async Task AddConnection()
    {
        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);
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
