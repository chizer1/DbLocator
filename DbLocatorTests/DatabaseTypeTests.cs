using DbLocator;
using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTypeTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleDatabaseTypesAndSearchByKeyWord()
    {
        var databaseTypeName1 = StringUtilities.RandomString(10);
        var databaseTypeId1 = await _dbLocator.AddDatabaseType(databaseTypeName1);

        var databaseTypeName2 = StringUtilities.RandomString(10);
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
        var databaseTypeName = StringUtilities.RandomString(10);
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
        var databaseTypeName1 = StringUtilities.RandomString(10);
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName1);

        var databaseTypeName2 = StringUtilities.RandomString(10);
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
}
