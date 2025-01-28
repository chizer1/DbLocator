using DbLocator.Domain;
using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTests
{
    private readonly DbLocator.DbLocator _dbLocator;
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

    [Fact]
    public async Task AddAndDeleteDatabase()
    {
        var database = await AddDatabaseAsync();

        await _dbLocator.DeleteDatabase(database.Id);

        var databases = await _dbLocator.GetDatabases();
        Assert.Empty(databases);
    }

    [Fact]
    public async Task AddAndUpdateDatabase()
    {
        var database = await AddDatabaseAsync();

        var updatedDatabaseName = $"[{StringUtilities.RandomString(10)}]";
        var updatedDatabaseUser = $"[{StringUtilities.RandomString(10)}]";
        await _dbLocator.UpdateDatabase(
            database.Id,
            updatedDatabaseName,
            updatedDatabaseUser,
            _databaseServerID,
            _databaseTypeId,
            Status.Inactive
        );

        var databases = (await _dbLocator.GetDatabases()).ToList();
        Assert.Single(databases);
        Assert.Equal(updatedDatabaseName, databases[0].Name);
        Assert.Equal(Status.Inactive, databases[0].Status);
    }

    private async Task<Database> AddDatabaseAsync()
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
