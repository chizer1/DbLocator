using DbLocator.Domain;
using DbLocatorTests.Fixtures;
using DbLocatorTests.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class TenantTests(DbLocatorFixture DbLocatorFixture)
{
    private readonly DbLocator.DbLocator _DbLocator = DbLocatorFixture.DbLocator;

    [Fact]
    public async Task AddMultipleTenantsAndSearchByKeyWord()
    {
        var TenantName = StringUtilities.RandomString(10);
        var TenantCode = StringUtilities.RandomString(3);
        var TenantId = await _DbLocator.AddTenant(TenantName, TenantCode, Status.Active);

        var TenantName2 = StringUtilities.RandomString(10);
        var TenantCode2 = StringUtilities.RandomString(3);
        var TenantId2 = await _DbLocator.AddTenant(TenantName2, TenantCode2, Status.Active);

        var Tenants = (await _DbLocator.GetTenants()).ToList();
        Assert.Single(Tenants);
        Assert.Equal(TenantName, Tenants[0].Name);
        Assert.Equal(TenantCode, Tenants[0].Code);
    }

    [Fact]
    public async Task AddAndDeleteTenant()
    {
        var TenantName = StringUtilities.RandomString(10);
        var TenantCode = StringUtilities.RandomString(3);
        var TenantId = await _DbLocator.AddTenant(TenantName, TenantCode, Status.Active);

        await _DbLocator.DeleteTenant(TenantId);
        var Tenant = await _DbLocator.GetTenants();
        Assert.Empty(Tenant);
    }

    [Fact]
    public async Task AddAndUpdateTenant()
    {
        var TenantName = StringUtilities.RandomString(10);
        var TenantCode = StringUtilities.RandomString(3);
        var TenantId = await _DbLocator.AddTenant(TenantName, TenantCode, Status.Active);

        var TenantName2 = StringUtilities.RandomString(10);
        var TenantCode2 = StringUtilities.RandomString(3);
        await _DbLocator.UpdateTenant(TenantId, TenantName2, TenantCode2, Status.Inactive);

        var oldTenants = (await _DbLocator.GetTenants()).ToList();
        Assert.Empty(oldTenants);

        var newTenants = (await _DbLocator.GetTenants()).ToList();
        Assert.Equal(TenantName2, newTenants[0].Name);
        Assert.Equal(TenantCode2, newTenants[0].Code);
        Assert.Equal(Status.Inactive, newTenants[0].Status);
    }
}
