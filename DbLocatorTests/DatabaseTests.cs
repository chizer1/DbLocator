using DbLocator;
using DbLocator.Domain;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTests
{
    private readonly Locator _dbLocator;
    private readonly int _databaseServerID;
    private readonly byte _databaseTypeId;

    public DatabaseTests(DbLocatorFixture dbLocatorFixture)
    {
        _dbLocator = dbLocatorFixture.DbLocator;
        _databaseServerID = _dbLocator
            .AddDatabaseServer("DatabaseServer", "127.168.1.1", null, null, false)
            .Result;
        _databaseTypeId = _dbLocator.AddDatabaseType("DatabaseType").Result;
    }

    private async Task<Database> AddDatabaseAsync(string databaseName)
    {
        var databaseUser = $"{databaseName}_App";
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        return (await _dbLocator.GetDatabases()).Single(db => db.Id == databaseId);
    }

    [Fact]
    public async Task AddMultipleDatabasesAndSearchByKeyWord()
    {
        var database1 = await AddDatabaseAsync("Acme1");
        var database2 = await AddDatabaseAsync("Acme2");

        var databases = (await _dbLocator.GetDatabases()).ToList();
        Assert.Equal(3, databases.Count);
        Assert.Contains(databases, db => db.Name == database1.Name);
        Assert.Contains(databases, db => db.Name == database2.Name);
    }
}
