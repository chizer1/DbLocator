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
        _databaseServerID = dbLocatorFixture.LocalhostServerId;
        _databaseTypeId = _dbLocator.AddDatabaseType(TestHelpers.GetRandomString()).Result;
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
        var dbNamePrefix = TestHelpers.GetRandomString();
        var database1 = await AddDatabaseAsync($"{dbNamePrefix}1");
        var database2 = await AddDatabaseAsync($"{dbNamePrefix}2");

        var databases = (await _dbLocator.GetDatabases()).ToList();
        Assert.Contains(databases, db => db.Name == database1.Name);
        Assert.Contains(databases, db => db.Name == database2.Name);
    }
}
