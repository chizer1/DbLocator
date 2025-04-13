using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTypeTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;
    private readonly DbLocatorCache _cache = dbLocatorFixture.LocatorCache;

    [Fact]
    public async Task AddMultipleDatabaseTypesAndSearchByKeyWord()
    {
        var databaseTypeName1 = TestHelpers.GetRandomString();
        var databaseTypeId1 = await _dbLocator.AddDatabaseType(databaseTypeName1);

        var databaseTypeName2 = TestHelpers.GetRandomString();
        var databaseTypeId2 = await _dbLocator.AddDatabaseType(databaseTypeName2);

        var databaseTypes = (await _dbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName1)
            .ToList();

        Assert.Single(databaseTypes);
        Assert.Equal(databaseTypeName1, databaseTypes[0].Name);
    }

    [Fact]
    public async Task AddAndDeleteDatabaseType()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        await _dbLocator.DeleteDatabaseType(databaseTypeId);
        var databaseTypes = (await _dbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName)
            .ToList();

        Assert.Empty(databaseTypes);
    }

    [Fact]
    public async Task AddAndUpdateDatabaseType()
    {
        var databaseTypeName1 = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName1);

        var databaseTypeName2 = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseType(databaseTypeId, databaseTypeName2);

        var oldDatabaseTypes = (await _dbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName1)
            .ToList();
        Assert.Empty(oldDatabaseTypes);

        var newDatabaseTypes = (await _dbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName2)
            .ToList();
        Assert.Single(newDatabaseTypes);
    }

    [Fact]
    public async Task VerifyDatabaseTypesAreCached()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseTypes = await _dbLocator.GetDatabaseTypes();
        Assert.Contains(databaseTypes, db => db.Name == databaseTypeName);

        var cachedDatabaseTypes = await _cache.GetCachedData<List<DatabaseType>>("databaseTypes");
        Assert.NotNull(cachedDatabaseTypes);
        Assert.Contains(cachedDatabaseTypes, db => db.Name == databaseTypeName);
    }
}
