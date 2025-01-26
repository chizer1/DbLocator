using DbLocator.Domain;
using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTests
{
    private readonly DbLocator.DbLocator _DbLocator;
    private readonly int _databaseServerID;
    private readonly byte _databaseTypeId;

    public DatabaseTests(DbLocatorFixture DbLocatorFixture)
    {
        _DbLocator = DbLocatorFixture.DbLocator;
        _databaseServerID = _DbLocator.AddDatabaseServer("DatabaseServer", "localhost").Result;
        _databaseTypeId = (byte)_DbLocator.AddDatabaseType("DatabaseType").Result;
    }

    [Fact]
    public async Task AddMultipleDatabasesAndSearchByKeyWord()
    {
        var databaseName = $"[{StringUtilities.RandomString(10)}]";
        var databaseUser = $"[{StringUtilities.RandomString(10)}]";
        var databaseId = await _DbLocator.AddDatabase(
            databaseName,
            databaseUser,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            false,
            true
        );

        var databaseName2 = $"[{StringUtilities.RandomString(10)}]";
        var databaseUser2 = $"[{StringUtilities.RandomString(10)}]";
        var databaseId2 = await _DbLocator.AddDatabase(
            databaseName2,
            databaseUser2,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            false,
            true
        );

        var databases = (await _DbLocator.GetDatabases()).ToList();
        Assert.Single(databases);
        Assert.Equal(databaseName, databases[0].Name);
    }

    [Fact]
    public async Task AddAndDeleteDatabase()
    {
        var databaseName = $"[{StringUtilities.RandomString(10)}]";
        var databaseUser = $"[{StringUtilities.RandomString(10)}]";
        var databaseId = await _DbLocator.AddDatabase(
            databaseName,
            databaseUser,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            false,
            true
        );

        await _DbLocator.DeleteDatabase(databaseId);

        var database = await _DbLocator.GetDatabases();
        Assert.Empty(database);
    }

    [Fact]
    public async Task AddAndUpdateDatabase()
    {
        var databaseName = $"[{StringUtilities.RandomString(10)}]";
        var databaseUser = $"[{StringUtilities.RandomString(10)}]";
        var databaseId = await _DbLocator.AddDatabase(
            databaseName,
            databaseUser,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            false,
            true
        );

        var databaseName2 = $"[{StringUtilities.RandomString(10)}]";
        var databaseUser2 = $"[{StringUtilities.RandomString(10)}]";
        await _DbLocator.UpdateDatabase(
            databaseId,
            databaseName2,
            databaseUser2,
            _databaseServerID,
            _databaseTypeId,
            Status.Inactive
        );

        var oldDatabases = (await _DbLocator.GetDatabases()).ToList();
        Assert.Empty(oldDatabases);

        var newDatabases = (await _DbLocator.GetDatabases()).ToList();
        Assert.Equal(databaseName2, newDatabases[0].Name);
        Assert.Equal(Status.Inactive, newDatabases[0].Status);
    }
}
