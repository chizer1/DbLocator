using DbLocator;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseTypeTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleDatabaseTypesAndSearchByKeyWord()
    {
        var databaseTypeName1 = "Client";
        var databaseTypeId1 = await _dbLocator.AddDatabaseType(databaseTypeName1);

        var databaseTypeName2 = "Forms";
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
        var databaseTypeName = "Logistics";
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
        var databaseTypeName1 = "BI";
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName1);

        var databaseTypeName2 = "Accounting";
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
