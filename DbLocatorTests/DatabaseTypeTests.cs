using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using FluentValidation;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTypeTests : IAsyncLifetime
{
    private readonly Locator _dbLocator;
    private readonly int _databaseServerId;
    private readonly DbLocatorCache _cache;
    private readonly List<DatabaseType> _testTypes = new();
    private readonly List<Database> _testDatabases = new();

    public DatabaseTypeTests(DbLocatorFixture dbLocatorFixture)
    {
        _dbLocator = dbLocatorFixture.DbLocator;
        _databaseServerId = dbLocatorFixture.LocalhostServerId;
        _cache = dbLocatorFixture.LocatorCache;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var type in _testTypes)
        {
            try
            {
                await _dbLocator.DeleteDatabaseType((byte)type.Id);
            }
            catch { }
        }
        _testTypes.Clear();

        foreach (var database in _testDatabases)
        {
            try
            {
                await _dbLocator.DeleteDatabase(database.Id);
            }
            catch { }
        }
        _testDatabases.Clear();

        await _cache.Remove("databaseTypes");
    }

    private async Task<DatabaseType> CreateDatabaseTypeAsync(string name = null)
    {
        name ??= TestHelpers.GetRandomString();
        var typeId = await _dbLocator.CreateDatabaseType(name);
        var type = await _dbLocator.GetDatabaseType(typeId);
        _testTypes.Add(type);
        return type;
    }

    private async Task<Database> CreateDatabaseAsync(
        string name = null,
        int? serverId = null,
        int? typeId = null,
        Status status = Status.Active)
    {
        name ??= TestHelpers.GetRandomString();
        serverId ??= _databaseServerId;
        typeId ??= (await CreateDatabaseTypeAsync()).Id;
        var databaseId = await _dbLocator.CreateDatabase(name, serverId.Value, (byte)typeId.Value, status);
        var database = await _dbLocator.GetDatabase(databaseId);
        _testDatabases.Add(database);
        return database;
    }

    #region Creation Tests
    [Fact]
    public async Task CreateMultipleDatabaseTypesAndSearchByKeyWord()
    {
        var type1 = await CreateDatabaseTypeAsync("TestType1");
        var type2 = await CreateDatabaseTypeAsync("TestType2");
        var type3 = await CreateDatabaseTypeAsync("AnotherType");

        var types = await _dbLocator.GetDatabaseTypes();
        Assert.Contains(types, t => t.Id == type1.Id);
        Assert.Contains(types, t => t.Id == type2.Id);
        Assert.Contains(types, t => t.Id == type3.Id);

        var searchResults = await _dbLocator.GetDatabaseTypes();
        Assert.Contains(searchResults, t => t.Id == type1.Id);
        Assert.Contains(searchResults, t => t.Id == type2.Id);
        Assert.DoesNotContain(searchResults, t => t.Id == type3.Id);
    }

    [Fact]
    public async Task CreateDatabaseTypeWithInvalidName_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _dbLocator.CreateDatabaseType("")
        );
    }
    #endregion

    #region Cache Tests
    [Fact]
    public async Task VerifyDatabaseTypesAreCached()
    {
        var type = await CreateDatabaseTypeAsync();

        var types = await _dbLocator.GetDatabaseTypes();
        Assert.Contains(types, t => t.Id == type.Id);

        var cachedTypes = await _cache.GetCachedData<List<DatabaseType>>("databaseTypes");
        Assert.NotNull(cachedTypes);
        Assert.Contains(cachedTypes, t => t.Id == type.Id);
    }

    [Fact]
    public async Task GetDatabaseTypes_FromCache()
    {
        var type = await CreateDatabaseTypeAsync();

        var firstCall = await _dbLocator.GetDatabaseTypes();
        Assert.Contains(firstCall, t => t.Id == type.Id);

        var secondCall = await _dbLocator.GetDatabaseTypes();
        Assert.Contains(secondCall, t => t.Id == type.Id);
        Assert.Equal(firstCall.Count, secondCall.Count);
    }

    [Fact]
    public async Task GetDatabaseType_RetrievesCachedType()
    {
        var type = await CreateDatabaseTypeAsync();

        var firstCall = await _dbLocator.GetDatabaseType((byte)type.Id);
        Assert.NotNull(firstCall);
        Assert.Equal(type.Id, firstCall.Id);

        var secondCall = await _dbLocator.GetDatabaseType((byte)type.Id);
        Assert.NotNull(secondCall);
        Assert.Equal(type.Id, secondCall.Id);
    }
    #endregion

    #region Update Tests
    [Fact]
    public async Task CreateAndUpdateDatabaseType()
    {
        var type = await CreateDatabaseTypeAsync();
        var newName = "UpdatedType";

        await _dbLocator.UpdateDatabaseType((byte)type.Id, newName);

        var updatedType = await _dbLocator.GetDatabaseType((byte)type.Id);
        Assert.Equal(newName, updatedType.Name);
    }

    [Fact]
    public async Task UpdateDatabaseTypeWithInvalidName_ThrowsArgumentException()
    {
        var type = await CreateDatabaseTypeAsync();

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _dbLocator.UpdateDatabaseType((byte)type.Id, "")
        );
    }

    [Fact]
    public async Task UpdateNonExistentDatabaseType_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.UpdateDatabaseType((byte)255, "NewName")
        );
    }
    #endregion

    #region Delete Tests
    [Fact]
    public async Task CreateAndDeleteDatabaseType()
    {
        var type = await CreateDatabaseTypeAsync();
        await _dbLocator.DeleteDatabaseType((byte)type.Id);

        var types = await _dbLocator.GetDatabaseTypes();
        Assert.DoesNotContain(types, t => t.Id == type.Id);
    }

    [Fact]
    public async Task DeleteNonExistentDatabaseType_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseType((byte)255)
        );
    }

    [Fact]
    public async Task CannotDeleteDatabaseTypeInUse()
    {
        var type = await CreateDatabaseTypeAsync();
        await CreateDatabaseAsync(typeId: type.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseType((byte)type.Id)
        );
    }
    #endregion

    #region Validation Tests
    [Fact]
    public async Task GetNonExistentDatabaseType_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetDatabaseType((byte)255)
        );
    }
    #endregion
}
