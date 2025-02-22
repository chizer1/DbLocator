using DbLocator;
using DbLocator.Domain;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class ConnectionTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddConnection()
    {
        var tenantName = "Tenant";
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = "DatabaseType";
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseServerName = "DBServer";
        var databaseServerIpAddress = "127.0.0.1";
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerIpAddress
        );

        var databaseName = "Database";
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            "database_user",
            "WvP26JM%6QP92y&PV",
            databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.AddConnection(tenantId, databaseId);

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);
    }
}
