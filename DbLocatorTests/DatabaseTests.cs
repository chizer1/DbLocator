using System.ComponentModel.DataAnnotations;
using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTests
{
    private readonly Locator _dbLocator;
    private readonly DbLocatorCache _cache;
    private readonly int _databaseServerID;
    private readonly byte _databaseTypeId;

    public DatabaseTests(DbLocatorFixture dbLocatorFixture)
    {
        _dbLocator = dbLocatorFixture.DbLocator;
        _databaseServerID = dbLocatorFixture.LocalhostServerId;
        _cache = dbLocatorFixture.LocatorCache;
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

    [Fact]
    public async Task VerifyDatabasesAreCached()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await AddDatabaseAsync(dbName);

        var databases = await _dbLocator.GetDatabases();
        Assert.Contains(databases, db => db.Name == database.Name);

        var cachedDatabases = await _cache.GetCachedData<List<Database>>("databases");
        Assert.NotNull(cachedDatabases);
        Assert.Contains(cachedDatabases, db => db.Name == database.Name);
    }

    [Fact]
    public async Task GetDatabaseById()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await AddDatabaseAsync(dbName);

        var retrievedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(retrievedDatabase);
        Assert.Equal(database.Id, retrievedDatabase.Id);
        Assert.Equal(database.Name, retrievedDatabase.Name);
        Assert.Equal(database.Server.Id, retrievedDatabase.Server.Id);
        Assert.Equal(database.Type.Id, retrievedDatabase.Type.Id);
        Assert.Equal(database.Status, retrievedDatabase.Status);
    }

    // [Fact]
    // public async Task UpdateDatabase()
    // {
    //     var dbName = TestHelpers.GetRandomString();
    //     var database = await AddDatabaseAsync(dbName);

    //     // Verify database exists
    //     var existingDatabase = await _dbLocator.GetDatabase(database.Id);
    //     Assert.NotNull(existingDatabase);

    //     var newName = TestHelpers.GetRandomString();
    //     var newDatabaseTypeName = TestHelpers.GetRandomString();
    //     var newDatabaseTypeId = await _dbLocator.AddDatabaseType(newDatabaseTypeName);

    //     await _dbLocator.UpdateDatabase(
    //         database.Id,
    //         newName,
    //         _databaseServerID,
    //         newDatabaseTypeId,
    //         Status.Inactive
    //     );

    //     var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
    //     Assert.NotNull(updatedDatabase);
    //     Assert.Equal(newName, updatedDatabase.Name);
    //     Assert.Equal(newDatabaseTypeId, updatedDatabase.Type.Id);
    //     Assert.Equal(Status.Inactive, updatedDatabase.Status);
    // }

    [Fact]
    public async Task DeleteDatabase()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await AddDatabaseAsync(dbName);

        await _dbLocator.DeleteDatabase(database.Id);

        var databases = await _dbLocator.GetDatabases();
        Assert.DoesNotContain(databases, db => db.Id == database.Id);
    }

    [Fact]
    public async Task GetNonExistentDatabase_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.GetDatabase(-1)
        );
    }

    [Fact]
    public async Task DeleteNonExistentDatabase_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabase(-1)
        );
    }

    [Fact]
    public async Task CannotDeleteDatabaseWithActiveConnections()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await AddDatabaseAsync(dbName);

        // Add a tenant and create a connection to the database
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);
        await _dbLocator.AddConnection(tenantId, database.Id);

        // Attempt to delete the database
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabase(database.Id)
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseWithInvalidServerId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await AddDatabaseAsync(dbName);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabase(
                    database.Id,
                    "new-name",
                    -1, // Invalid server ID
                    _databaseTypeId,
                    Status.Active
                )
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseWithInvalidTypeId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await AddDatabaseAsync(dbName);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabase(
                    database.Id,
                    "new-name",
                    _databaseServerID,
                    255, // Invalid type ID
                    Status.Active
                )
        );
    }

    [Fact]
    public async Task AddDatabase_WithNonExistentServer_ThrowsValidationException()
    {
        var dbName = TestHelpers.GetRandomString();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.AddDatabase(
                    dbName,
                    4, // Non-existent server ID
                    _databaseTypeId,
                    Status.Active
                )
        );
    }

    [Fact]
    public async Task AddDatabase_WithNonExistentDatabaseType_ThrowsValidationException()
    {
        var dbName = TestHelpers.GetRandomString();
        var newIpAddress = TestHelpers.GetRandomIpAddressString();
        var newHostName = "updated-host123";
        var newFqdn = "updated-host123.example.com";

        var dbServerId = await _dbLocator.AddDatabaseServer(
            "NewDatabaseServer123",
            newIpAddress,
            newHostName,
            newFqdn,
            false
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.AddDatabase(
                    dbName,
                    dbServerId,
                    56, // Non-existent database type ID
                    Status.Active
                )
        );
    }
}
