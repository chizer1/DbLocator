using DbLocator;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseServerTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;

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
}
