using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTypeTests(DbLocatorFixture DbLocatorFixture)
{
    private readonly DbLocator.DbLocator _DbLocator = DbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleDatabaseTypesAndSearchByKeyWord()
    {
        var databaseTypeName = StringUtilities.RandomString(10);
        var databaseTypeId = await _DbLocator.AddDatabaseType(databaseTypeName);

        var databaseTypeName2 = StringUtilities.RandomString(10);
        var databaseTypeId2 = await _DbLocator.AddDatabaseType(databaseTypeName2);

        var databaseTypes = (await _DbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName)
            .ToList();

        Assert.Single(databaseTypes);
        Assert.Equal(databaseTypeName, databaseTypes[0].Name);
    }

    [Fact]
    public async Task AddAndDeleteDatabaseType()
    {
        var databaseTypeName = StringUtilities.RandomString(10);
        var databaseTypeId = await _DbLocator.AddDatabaseType(databaseTypeName);

        await _DbLocator.DeleteDatabaseType(databaseTypeId);
        var databaseType = (await _DbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName)
            .ToList();

        Assert.Empty(databaseType);
    }

    [Fact]
    public async Task AddAndUpdateDatabaseType()
    {
        var databaseTypeName = StringUtilities.RandomString(10);
        var databaseTypeId = await _DbLocator.AddDatabaseType(databaseTypeName);

        var databaseTypeName2 = StringUtilities.RandomString(10);
        await _DbLocator.UpdateDatabaseType(databaseTypeId, databaseTypeName2);

        var oldDatabaseTypes = (await _DbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName)
            .ToList();
        Assert.Empty(oldDatabaseTypes);

        var newDatabaseTypes = (await _DbLocator.GetDatabaseTypes())
            .Where(x => x.Name == databaseTypeName2)
            .ToList();
        Assert.Single(newDatabaseTypes);
    }
}
