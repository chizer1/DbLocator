using DbLocator;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseServerTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleDatabaseServersAndSearchByKeyWord()
    {
        var databaseServerName = "DBServer";
        var databaseServerIpAddress = "192.168.1.1";
        await _dbLocator.AddDatabaseServer(databaseServerName, databaseServerIpAddress);

        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(databaseServerName, databaseServers[0].Name);
        Assert.Equal(databaseServerIpAddress, databaseServers[0].IpAddress);
    }

    [Fact]
    public async Task AddAndDeleteDatabaseServer()
    {
        var databaseServerName = "DBServer";
        var databaseServerIpAddress = "192.168.1.1";
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerIpAddress
        );

        await _dbLocator.DeleteDatabaseServer(databaseServerId);
        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.Empty(databaseServers);
    }
}
