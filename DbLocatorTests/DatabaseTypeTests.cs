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
    private readonly int _databaseServerID = dbLocatorFixture.LocalhostServerId;

    [Fact]
    public async Task CreateMultipleDatabaseTypesAndSearchByKeyWord()
    {
        var databaseTypeName1 = TestHelpers.GetRandomString();
        var databaseTypeId1 = await _dbLocator.CreateDatabaseType(databaseTypeName1);

        var databaseTypeName2 = TestHelpers.GetRandomString();
        var databaseTypeId2 = await _dbLocator.CreateDatabaseType(databaseTypeName2);

        var databaseTypes = (await _dbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName1)
            .ToList();

        Assert.Single(databaseTypes);
        Assert.Equal(databaseTypeName1, databaseTypes[0].Name);
    }

    [Fact]
    public async Task CreateAndDeleteDatabaseType()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        await _dbLocator.DeleteDatabaseType(databaseTypeId);
        var databaseTypes = (await _dbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName)
            .ToList();

        Assert.Empty(databaseTypes);
    }

    [Fact]
    public async Task CreateAndUpdateDatabaseType()
    {
        var databaseTypeName1 = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName1);

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
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseTypes = await _dbLocator.GetDatabaseTypes();
        Assert.Contains(databaseTypes, db => db.Name == databaseTypeName);

        var cachedDatabaseTypes = await _cache.GetCachedData<List<DatabaseType>>("databaseTypes");
        Assert.NotNull(cachedDatabaseTypes);
        Assert.Contains(cachedDatabaseTypes, db => db.Name == databaseTypeName);
    }

    [Fact]
    public async Task GetDatabaseTypeById()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var databaseType = await _dbLocator.GetDatabaseType(databaseTypeId);
        Assert.NotNull(databaseType);
        Assert.Equal(databaseTypeId, databaseType.Id);
        Assert.Equal(databaseTypeName, databaseType.Name);
    }

    [Fact]
    public async Task CannotCreateDuplicateDatabaseTypeName()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        await _dbLocator.CreateDatabaseType(databaseTypeName);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.CreateDatabaseType(databaseTypeName)
        );
    }

    [Fact]
    public async Task CannotCreateDatabaseTypeWithNameTooLong()
    {
        var longName = new string('a', 21); // Max length is 20
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.CreateDatabaseType(longName)
        );
    }

    [Fact]
    public async Task CannotDeleteDatabaseTypeInUse()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        // Create a database using this type
        var databaseName = TestHelpers.GetRandomString();
        await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerID,
            databaseTypeId,
            Status.Active
        );

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseType(databaseTypeId)
        );
    }

    [Fact]
    public async Task GetNonExistentDatabaseType_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetDatabaseType(255)
        );
    }

    [Fact]
    public async Task DeleteNonExistentDatabaseType_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseType(255)
        );
    }

    [Fact]
    public async Task UpdateNonExistentDatabaseType_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.UpdateDatabaseType(255, "NewName")
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseTypeWithNameTooLong()
    {
        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.CreateDatabaseType(databaseTypeName);

        var longName = new string('a', 21); // Max length is 20
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.UpdateDatabaseType(databaseTypeId, longName)
        );
    }

    [Fact]
    public async Task CannotUpdateDatabaseTypeToDuplicateName()
    {
        var databaseTypeName1 = TestHelpers.GetRandomString();
        var databaseTypeId1 = await _dbLocator.CreateDatabaseType(databaseTypeName1);

        var databaseTypeName2 = TestHelpers.GetRandomString();
        var databaseTypeId2 = await _dbLocator.CreateDatabaseType(databaseTypeName2);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.UpdateDatabaseType(databaseTypeId2, databaseTypeName1)
        );
    }

    [Fact]
    public async Task GetDatabaseType_ReturnsCachedData()
    {
        var typeName = TestHelpers.GetRandomString();
        var typeId = await _dbLocator.CreateDatabaseType(typeName);

        var type = await _dbLocator.GetDatabaseType(typeId);
        Assert.NotNull(type);

        await _dbLocator.DeleteDatabaseType(typeId);

        var cachedType = await _dbLocator.GetDatabaseType(typeId);

        Assert.NotNull(cachedType);
        Assert.Equal(typeId, cachedType.Id);
        Assert.Equal(typeName, cachedType.Name);
    }
}
