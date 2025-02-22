using DbLocator;
using DbLocator.Domain;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class TenantTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleTenantsAndSearchByKeyWord()
    {
        var tenantName1 = "Acme Industries";
        var tenantCode1 = "acme";
        await _dbLocator.AddTenant(tenantName1, tenantCode1, Status.Active);

        var tenantName2 = "Delta Corp";
        var tenantCode2 = "delta";
        await _dbLocator.AddTenant(tenantName2, tenantCode2, Status.Active);

        var tenants = (await _dbLocator.GetTenants()).ToList();
        Assert.Equal(3, tenants.Count);
        Assert.Contains(tenants, t => t.Name == tenantName1 && t.Code == tenantCode1);
        Assert.Contains(tenants, t => t.Name == tenantName2 && t.Code == tenantCode2);
    }
}
