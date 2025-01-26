using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseServerTests(DbLocatorFixture DbLocatorFixture)
{
    private readonly DbLocator.DbLocator _DbLocator = DbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleDatabaseServersAndSearchByKeyWord()
    {
        var databaseServerName = StringUtilities.RandomString(10);
        var databaseServerLocation = StringUtilities.RandomString(10);
        var databaseServerId = await _DbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerLocation
        );

        var databaseServerName2 = StringUtilities.RandomString(10);
        var databaseServerLocation2 = StringUtilities.RandomString(10);
        var databaseServerId2 = await _DbLocator.AddDatabaseServer(
            databaseServerName2,
            databaseServerLocation2
        );

        var databaseServers = (await _DbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(databaseServerName, databaseServers[0].Name);
        Assert.Equal(databaseServerLocation, databaseServers[0].IpAddress);
    }

    [Fact]
    public async Task AddAndDeleteDatabaseServer()
    {
        var databaseServerName = StringUtilities.RandomString(10);
        var databaseServerLocation = StringUtilities.RandomString(10);
        var databaseServerId = await _DbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerLocation
        );

        await _DbLocator.DeleteDatabaseServer(databaseServerId);
        var databaseServer = (await _DbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();
        Assert.Empty(databaseServer);
    }

    [Fact]
    public async Task AddAndUpdateDatabaseServer()
    {
        var databaseServerName = StringUtilities.RandomString(10);
        var databaseServerLocation = StringUtilities.RandomString(10);
        var databaseServerId = await _DbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerLocation
        );

        var databaseServerName2 = StringUtilities.RandomString(10);
        var databaseServerLocation2 = StringUtilities.RandomString(10);
        await _DbLocator.UpdateDatabaseServer(
            databaseServerId,
            databaseServerName2,
            databaseServerLocation2
        );

        var databaseServers = (await _DbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName2)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(databaseServerName2, databaseServers[0].Name);
        Assert.Equal(databaseServerLocation2, databaseServers[0].IpAddress);
    }
}
