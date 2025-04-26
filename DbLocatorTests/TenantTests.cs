using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class TenantTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;
    private readonly DbLocatorCache _cache = dbLocatorFixture.LocatorCache;

    [Fact]
    public async Task AddMultipleTenantsAndSearchByKeyWord()
    {
        var tenantName1 = TestHelpers.GetRandomString();
        var tenantCode1 = TestHelpers.GetRandomString();
        await _dbLocator.AddTenant(tenantName1, tenantCode1, Status.Active);

        var tenantName2 = TestHelpers.GetRandomString();
        var tenantCode2 = TestHelpers.GetRandomString();
        await _dbLocator.AddTenant(tenantName2, tenantCode2, Status.Active);

        var tenants = (await _dbLocator.GetTenants()).ToList();
        Assert.Contains(tenants, t => t.Name == tenantName1 && t.Code == tenantCode1);
        Assert.Contains(tenants, t => t.Name == tenantName2 && t.Code == tenantCode2);
    }

    [Fact]
    public async Task VerifyTenantsAreCached()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        var tenants = await _dbLocator.GetTenants();
        Assert.Contains(tenants, t => t.Id == tenantId);

        var cachedTenants = await _cache.GetCachedData<List<Tenant>>("tenants");
        Assert.NotNull(cachedTenants);
        Assert.Contains(cachedTenants, t => t.Id == tenantId);
    }

    [Fact]
    public async Task GetTenantById()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        var tenant = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Equal(tenantCode, tenant.Code);
        Assert.Equal(Status.Active, tenant.Status);
    }

    [Fact]
    public async Task GetTenantByCode()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        var tenant = await _dbLocator.GetTenant(tenantCode);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Equal(tenantCode, tenant.Code);
        Assert.Equal(Status.Active, tenant.Status);
    }

    [Fact]
    public async Task UpdateTenant()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        var newName = TestHelpers.GetRandomString();
        var newCode = TestHelpers.GetRandomString();
        await _dbLocator.UpdateTenant(tenantId, newName, newCode, Status.Inactive);

        var updatedTenant = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(newName, updatedTenant.Name);
        Assert.Equal(newCode, updatedTenant.Code);
        Assert.Equal(Status.Inactive, updatedTenant.Status);
    }

    [Fact]
    public async Task DeleteTenant()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        await _dbLocator.DeleteTenant(tenantId);

        var tenants = await _dbLocator.GetTenants();
        Assert.DoesNotContain(tenants, t => t.Id == tenantId);
    }

    [Fact]
    public async Task GetNonExistentTenantById_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.GetTenant(-1)
        );
    }

    [Fact]
    public async Task GetNonExistentTenantByCode_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetTenant("NONEXISTENT")
        );
    }

    [Fact]
    public async Task DeleteNonExistentTenant_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteTenant(-1)
        );
    }

    [Fact]
    public async Task CannotDeleteTenantWithActiveConnections()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        // Add a database and create a connection to the tenant
        var dbName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(dbName, 1, 1);
        await _dbLocator.AddConnection(tenantId, databaseId);

        // Attempt to delete the tenant
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteTenant(tenantId)
        );
    }

    // add two tests... get tenant by id (cached) and get tenant by code (cached)
    [Fact]
    public async Task GetTenantById_Cached()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        var tenant = await _cache.GetCachedData<Tenant>(tenantId.ToString());
        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Equal(tenantCode, tenant.Code);
        Assert.Equal(Status.Active, tenant.Status);
    }

    [Fact]
    public async Task GetTenantByCode_Cached()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        await _dbLocator.AddTenant(tenantName, tenantCode, Status.Active);

        var tenant = await _cache.GetCachedData<Tenant>(tenantCode);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Equal(tenantCode, tenant.Code);
        Assert.Equal(Status.Active, tenant.Status);
    }
}
