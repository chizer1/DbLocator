using DbLocator;
using DbLocator.Domain;
using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

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
        _databaseServerID = _dbLocator.AddDatabaseServer("DatabaseServer", "localhost").Result;
        _databaseTypeId = (byte)_dbLocator.AddDatabaseType("DatabaseType").Result;
    }

    [Fact]
    public async Task AddMultipleDatabasesAndSearchByKeyWord()
    {
        var database1 = await AddDatabaseAsync();
        var database2 = await AddDatabaseAsync();

        var databases = (await _dbLocator.GetDatabases()).ToList();
        Assert.Equal(2, databases.Count);
        Assert.Contains(databases, db => db.Name == database1.Name);
        Assert.Contains(databases, db => db.Name == database2.Name);
    }

    public async Task<Database> AddDatabaseAsync()
    {
        var databaseName = $"[{StringUtilities.RandomString(10)}]";
        var databaseUser = $"[{StringUtilities.RandomString(10)}]";
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            databaseUser,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        return (await _dbLocator.GetDatabases()).Single(db => db.Id == databaseId);
    }
}
