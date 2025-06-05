using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTests : IAsyncLifetime
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
        _databaseTypeId = _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString()).Result;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _cache.Remove("databases");
    }

    private async Task<Database> CreateDatabaseAsync(
        string databaseName = null,
        bool affectDatabase = true,
        int? serverId = null,
        byte? typeId = null
    )
    {
        databaseName ??= TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            serverId ?? _databaseServerID,
            typeId ?? _databaseTypeId,
            affectDatabase
        );

        return (await _dbLocator.GetDatabases()).Single(db => db.Id == databaseId);
    }

    #region Creation Tests
    [Fact]
    public async Task CreateMultipleDatabasesAndSearchByKeyWord()
    {
        var dbNamePrefix = TestHelpers.GetRandomString();
        var database1 = await CreateDatabaseAsync($"{dbNamePrefix}1");
        var database2 = await CreateDatabaseAsync($"{dbNamePrefix}2");

        var databases = (await _dbLocator.GetDatabases()).ToList();
        Assert.Contains(databases, db => db.Name == database1.Name);
        Assert.Contains(databases, db => db.Name == database2.Name);
    }

    [Fact]
    public async Task CreateDatabase_WithDbNameServerIdAndTypeId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        Assert.NotNull(database);
        Assert.Equal(dbName, database.Name);
        Assert.Equal(_databaseServerID, database.Server.Id);
        Assert.Equal(_databaseTypeId, database.Type.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateDatabase_WithAffectDatabaseFlag_SetsCorrectStatus(bool affectDatabase)
    {
        var database = await CreateDatabaseAsync(affectDatabase: affectDatabase);

        var databases = await _dbLocator.GetDatabases();
        Assert.Contains(databases, db => db.Name == database.Name);
        Assert.Equal(Status.Active, database.Status);
    }
    #endregion

    #region Cache Tests
    [Fact]
    public async Task VerifyDatabasesAreCached()
    {
        var database = await CreateDatabaseAsync();

        var databases = await _dbLocator.GetDatabases();
        Assert.Contains(databases, db => db.Name == database.Name);

        var cachedDatabases = await _cache.GetCachedData<List<Database>>("databases");
        Assert.NotNull(cachedDatabases);
        Assert.Contains(cachedDatabases, db => db.Name == database.Name);
    }

    [Fact]
    public async Task GetDatabaseById_ReturnsFromCache()
    {
        var database = await CreateDatabaseAsync();

        // First call should populate cache
        var firstCall = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(firstCall);

        // Second call should use cache
        var secondCall = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(secondCall);
        Assert.Equal(firstCall.Id, secondCall.Id);
    }
    #endregion

    #region Update Tests
    [Fact]
    public async Task UpdateDatabase_ServerId()
    {
        var database = await CreateDatabaseAsync();
        var newServer = await CreateDatabaseServerAsync();

        await _dbLocator.UpdateDatabase(database.Id, null, newServer.Id, null, null, null, true);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(newServer.Id, updatedDatabase.Server.Id);
    }

    [Fact]
    public async Task UpdateDatabase_TypeId()
    {
        var database = await CreateDatabaseAsync();
        var newTypeId = await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString());

        await _dbLocator.UpdateDatabase(database.Id, null, null, newTypeId, null, null, true);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(newTypeId, updatedDatabase.Type.Id);
    }

    [Fact]
    public async Task UpdateDatabase_DatabaseName()
    {
        var database = await CreateDatabaseAsync();
        var newName = TestHelpers.GetRandomString();

        await _dbLocator.UpdateDatabase(database.Id, newName, null, null, null, null, true);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(newName, updatedDatabase.Name);
    }

    [Theory]
    [InlineData(Status.Active)]
    [InlineData(Status.Inactive)]
    public async Task UpdateDatabase_UpdateStatus(Status newStatus)
    {
        var database = await CreateDatabaseAsync();

        await _dbLocator.UpdateDatabase(database.Id, null, null, null, null, newStatus, true);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(newStatus, updatedDatabase.Status);
    }
    #endregion

    #region Validation Tests
    [Fact]
    public async Task CreateDatabase_WithNonExistentServer_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await CreateDatabaseAsync(serverId: 77)
        );
    }

    [Fact]
    public async Task CreateDatabase_WithNonExistentDatabaseType_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await CreateDatabaseAsync(typeId: 56)
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseWithInvalidServerId()
    {
        var database = await CreateDatabaseAsync();

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabase(
                    database.Id,
                    "new-name",
                    -1,
                    _databaseTypeId,
                    null,
                    Status.Active,
                    true
                )
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseWithInvalidTypeId()
    {
        var database = await CreateDatabaseAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabase(
                    database.Id,
                    "new-name",
                    _databaseServerID,
                    255,
                    false,
                    Status.Active,
                    true
                )
        );
    }
    #endregion

    #region Delete Tests
    [Fact]
    public async Task DeleteDatabase()
    {
        var database = await CreateDatabaseAsync();
        await _dbLocator.DeleteDatabase(database.Id);

        var databases = await _dbLocator.GetDatabases();
        Assert.DoesNotContain(databases, db => db.Id == database.Id);
    }

    [Fact]
    public async Task GetNonExistentDatabase_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetDatabase(999999)
        );
    }

    [Fact]
    public async Task DeleteNonExistentDatabase_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabase(999999)
        );
    }

    [Fact]
    public async Task CannotDeleteDatabaseWithActiveConnections()
    {
        var database = await CreateDatabaseAsync();
        var tenantId = await _dbLocator.CreateTenant(TestHelpers.GetRandomString());
        await _dbLocator.CreateConnection(tenantId, database.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabase(database.Id)
        );
    }
    #endregion

    private async Task<DatabaseServer> CreateDatabaseServerAsync()
    {
        var serverId = await _dbLocator.CreateDatabaseServer(
            TestHelpers.GetRandomString(),
            null,
            TestHelpers.GetRandomIpAddressString(),
            null,
            false
        );
        return (await _dbLocator.GetDatabaseServers()).Single(s => s.Id == serverId);
    }
}
