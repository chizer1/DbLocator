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
        _databaseTypeId = _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString()).Result;
    }

    private async Task<Database> CreateDatabaseAsync(string databaseName)
    {
        var databaseUser = $"{databaseName}_App";
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        return (await _dbLocator.GetDatabases()).Single(db => db.Id == databaseId);
    }

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
    public async Task VerifyDatabasesAreCached()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

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
        var database = await CreateDatabaseAsync(dbName);

        var retrievedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(retrievedDatabase);
        Assert.Equal(database.Id, retrievedDatabase.Id);
        Assert.Equal(database.Name, retrievedDatabase.Name);
        Assert.Equal(database.Server.Id, retrievedDatabase.Server.Id);
        Assert.Equal(database.Type.Id, retrievedDatabase.Type.Id);
        Assert.Equal(database.Status, retrievedDatabase.Status);
    }

    [Fact]
    public async Task DeleteDatabase()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

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
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);
        await _dbLocator.CreateConnection(tenantId, database.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabase(database.Id)
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseWithInvalidServerId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabase(
                    database.Id,
                    "new-name",
                    -1,
                    _databaseTypeId,
                    Status.Active
                )
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseWithInvalidTypeId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabase(
                    database.Id,
                    "new-name",
                    _databaseServerID,
                    255,
                    Status.Active
                )
        );
    }

    [Fact]
    public async Task CreateDatabase_WithNonExistentServer_ThrowsValidationException()
    {
        var dbName = TestHelpers.GetRandomString();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.CreateDatabase(dbName, 77, _databaseTypeId, Status.Active)
        );
    }

    [Fact]
    public async Task CreateDatabase_WithNonExistentDatabaseType_ThrowsValidationException()
    {
        var dbName = TestHelpers.GetRandomString();
        var newIpAddress = TestHelpers.GetRandomIpAddressString();

        var dbServerId = await _dbLocator.CreateDatabaseServer(
            "testservername",
            false,
            null,
            newIpAddress,
            null
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.CreateDatabase(dbName, dbServerId, 56, Status.Active)
        );
    }

    [Fact]
    public async Task CreateDatabase_WithDbNameServerIdAndTypeId()
    {
        var dbName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            dbName,
            _databaseServerID,
            _databaseTypeId,
            false
        );

        var database = await _dbLocator.GetDatabase(databaseId);
        Assert.NotNull(database);
        Assert.Equal(dbName, database.Name);
        Assert.Equal(_databaseServerID, database.Server.Id);
        Assert.Equal(_databaseTypeId, database.Type.Id);
    }

    [Fact]
    public async Task CreateDatabase_WithDbNameServerIdAndTypeIdAndCreate()
    {
        var dbName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            dbName,
            _databaseServerID,
            _databaseTypeId,
            true
        );

        var database = await _dbLocator.GetDatabase(databaseId);
        Assert.NotNull(database);
        Assert.Equal(dbName, database.Name);
        Assert.Equal(_databaseServerID, database.Server.Id);
        Assert.Equal(_databaseTypeId, database.Type.Id);
    }

    [Fact]
    public async Task UpdateDatabase_ServerId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        var newIpAddress = TestHelpers.GetRandomIpAddressString();

        var newServerId = await _dbLocator.CreateDatabaseServer(
            "testservername987",
            false,
            null,
            newIpAddress,
            null
        );

        await _dbLocator.UpdateDatabase(database.Id, newServerId);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(newServerId, updatedDatabase.Server.Id);
    }

    [Fact]
    public async Task UpdateDatabase_TypeId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        var newDatabaseTypeName = TestHelpers.GetRandomString();
        var newDatabaseTypeId = await _dbLocator.CreateDatabaseType(newDatabaseTypeName);

        await _dbLocator.UpdateDatabase(database.Id, newDatabaseTypeId);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(newDatabaseTypeId, updatedDatabase.Type.Id);
    }

    [Fact]
    public async Task UpdateDatabase_DatabaseName()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        var newDatabaseName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabase(database.Id, newDatabaseName);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(newDatabaseName, updatedDatabase.Name);
    }

    [Fact]
    public async Task UpdateDatabase_UpdateStatus()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        await _dbLocator.UpdateDatabase(database.Id, Status.Inactive);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.Equal(Status.Inactive, updatedDatabase.Status);
    }

    [Fact]
    public async Task DeleteDatabase_DeleteDatabase()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        await _dbLocator.DeleteDatabase(database.Id, true);

        var databases = await _dbLocator.GetDatabases();
        Assert.DoesNotContain(databases, db => db.Id == database.Id);
    }

    [Fact]
    public async Task UpdateDatabase_WithInvalidDatabaseServerId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.UpdateDatabase(database.Id, 34345)
        );
    }

    [Fact]
    public async Task UpdateDatabase_WithInvalidDatabaseTypeId()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.UpdateDatabase(database.Id, 2387)
        );
    }

    [Fact]
    public async Task CreateDatabase_WithTrustedConnection_CreatesDatabaseWithTrustedConnection()
    {
        var dbName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            dbName,
            _databaseServerID,
            _databaseTypeId,
            true,
            true
        );

        var database = await _dbLocator.GetDatabase(databaseId);

        Assert.NotNull(database);
        Assert.Equal(dbName, database.Name);
        Assert.Equal(_databaseServerID, database.Server.Id);
        Assert.Equal(_databaseTypeId, database.Type.Id);
        Assert.True(database.UseTrustedConnection);
    }

    [Fact]
    public async Task UpdateDatabase_WithTrustedConnection_UpdatesDatabaseTrustedConnection()
    {
        var dbName = TestHelpers.GetRandomString();
        var database = await CreateDatabaseAsync(dbName);
        Assert.False(database.UseTrustedConnection);

        await _dbLocator.UpdateDatabase(database.Id, true);

        var updatedDatabase = await _dbLocator.GetDatabase(database.Id);
        Assert.NotNull(updatedDatabase);
        Assert.True(updatedDatabase.UseTrustedConnection);
    }

    [Fact]
    public async Task CacheData_WithStringData_CachesStringDirectly()
    {
        var cacheKey = "test_string_cache";
        var stringData = "test string data";

        await _cache.CacheData(cacheKey, stringData);

        var cachedData = await _cache.GetCachedData<string>(cacheKey);
        Assert.Equal(stringData, cachedData);
    }

    [Fact]
    public async Task TryClearConnectionStringFromCache_WithTenantCode_RemovesMatchingKeys()
    {
        var tenantCode = TestHelpers.GetRandomString();
        var queryString =
            @$"TenantId:,
            DatabaseTypeId:,
            ConnectionId:,
            TenantCode:{tenantCode},
            Roles:None";
        var cacheKey = $"connection:{queryString}";
        await _cache.CacheConnectionString(cacheKey, "test connection string");

        await _cache.TryClearConnectionStringFromCache(tenantCode: tenantCode);

        var cachedData = await _cache.GetCachedData<string>(cacheKey);
        Assert.Null(cachedData);
    }

    [Fact]
    public async Task TryClearConnectionStringFromCache_WithDatabaseRoles_RemovesMatchingKeys()
    {
        var roles = new[] { DatabaseRole.DataReader, DatabaseRole.DataWriter };
        var rolesString = string.Join(",", roles);
        var queryString =
            @$"TenantId:,
            DatabaseTypeId:,
            ConnectionId:,
            TenantCode:,
            Roles:{rolesString}";
        var cacheKey = $"connection:{queryString}";
        await _cache.CacheConnectionString(cacheKey, "test connection string");

        await _cache.TryClearConnectionStringFromCache(roles: roles);

        var cachedData = await _cache.GetCachedData<string>(cacheKey);
        Assert.Null(cachedData);
    }
}
