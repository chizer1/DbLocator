using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseServerTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly DbLocator.DbLocator _dbLocator = dbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleDatabaseServersAndSearchByKeyWord()
    {
        var databaseServerName = StringUtilities.RandomString(10);
        var databaseServerLocation = StringUtilities.RandomString(10);
        await _dbLocator.AddDatabaseServer(databaseServerName, databaseServerLocation);

        var databaseServerName2 = StringUtilities.RandomString(10);
        var databaseServerLocation2 = StringUtilities.RandomString(10);
        await _dbLocator.AddDatabaseServer(databaseServerName2, databaseServerLocation2);

        var databaseServers = (await _dbLocator.GetDatabaseServers())
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
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerLocation
        );

        await _dbLocator.DeleteDatabaseServer(databaseServerId);
        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName)
            .ToList();

        Assert.NotEmpty(databaseServers);
    }

    [Fact]
    public async Task AddAndUpdateDatabaseServer()
    {
        var databaseServerName = StringUtilities.RandomString(10);
        var databaseServerLocation = StringUtilities.RandomString(10);
        var databaseServerId = await _dbLocator.AddDatabaseServer(
            databaseServerName,
            databaseServerLocation
        );

        var databaseServerName2 = StringUtilities.RandomString(10);
        var databaseServerLocation2 = StringUtilities.RandomString(10);
        await _dbLocator.UpdateDatabaseServer(
            databaseServerId,
            databaseServerName2,
            databaseServerLocation2
        );

        var databaseServers = (await _dbLocator.GetDatabaseServers())
            .Where(x => x.Name == databaseServerName2)
            .ToList();

        Assert.Single(databaseServers);
        Assert.Equal(databaseServerName2, databaseServers[0].Name);
        Assert.Equal(databaseServerLocation2, databaseServers[0].IpAddress);
    }
}
