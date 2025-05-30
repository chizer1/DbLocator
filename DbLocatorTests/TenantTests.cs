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
    public async Task CreateMultipleTenantsAndSearchByKeyWord()
    {
        var tenantName1 = TestHelpers.GetRandomString();
        var tenantCode1 = TestHelpers.GetRandomString();
        await _dbLocator.CreateTenant(tenantName1, tenantCode1, Status.Active);

        var tenantName2 = TestHelpers.GetRandomString();
        var tenantCode2 = TestHelpers.GetRandomString();
        await _dbLocator.CreateTenant(tenantName2, tenantCode2, Status.Active);

        var tenants = (await _dbLocator.GetTenants()).ToList();
        Assert.Contains(tenants, t => t.Name == tenantName1 && t.Code == tenantCode1);
        Assert.Contains(tenants, t => t.Name == tenantName2 && t.Code == tenantCode2);
    }

    [Fact]
    public async Task VerifyTenantsAreCached()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

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
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

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
        await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

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
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var newName = TestHelpers.GetRandomString();
        var newCode = TestHelpers.GetRandomString();
        await _dbLocator.UpdateTenant(tenantId, newName, newCode);
        await _dbLocator.UpdateTenant(tenantId, Status.Inactive);

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
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        await _dbLocator.DeleteTenant(tenantId);

        var tenants = await _dbLocator.GetTenants();
        Assert.DoesNotContain(tenants, t => t.Id == tenantId);
    }

    [Fact]
    public async Task GetNonExistentTenantById_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _dbLocator.GetTenant(-1));
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
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var dbName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(dbName, 1, 1, false);
        await _dbLocator.CreateConnection(tenantId, databaseId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteTenant(tenantId)
        );
    }

    [Fact]
    public async Task GetTenantById_Cached()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var tenantFromDb = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(tenantId, tenantFromDb.Id);
        Assert.Equal(tenantName, tenantFromDb.Name);
        Assert.Equal(tenantCode, tenantFromDb.Code);
        Assert.Equal(Status.Active, tenantFromDb.Status);

        var tenantFromCache = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(tenantId, tenantFromCache.Id);
        Assert.Equal(tenantName, tenantFromCache.Name);
        Assert.Equal(tenantCode, tenantFromCache.Code);
        Assert.Equal(Status.Active, tenantFromCache.Status);
    }

    [Fact]
    public async Task GetTenantByCode_Cached()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var tenantFromDb = await _dbLocator.GetTenant(tenantCode);
        Assert.Equal(tenantName, tenantFromDb.Name);
        Assert.Equal(tenantCode, tenantFromDb.Code);
        Assert.Equal(Status.Active, tenantFromDb.Status);

        var tenantFromCache = await _dbLocator.GetTenant(tenantCode);
        Assert.Equal(tenantName, tenantFromCache.Name);
        Assert.Equal(tenantCode, tenantFromCache.Code);
        Assert.Equal(Status.Active, tenantFromCache.Status);
    }

    [Fact]
    public async Task CreateTenantWithOnlyName()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);

        var tenant = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Null(tenant.Code);
        Assert.Equal(Status.Active, tenant.Status);
    }

    [Fact]
    public async Task UpdateTenant_StatusOnly()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        await _dbLocator.UpdateTenant(tenantId, Status.Inactive);

        var updatedTenant = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(Status.Inactive, updatedTenant.Status);
        Assert.Equal(tenantName, updatedTenant.Name);
        Assert.Equal(tenantCode, updatedTenant.Code);
    }

    [Fact]
    public async Task UpdateTenant_NameOnly()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateTenant(tenantId, newName);

        var updatedTenant = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(newName, updatedTenant.Name);
        Assert.Equal(tenantCode, updatedTenant.Code);
        Assert.Equal(Status.Active, updatedTenant.Status);
    }

    [Fact]
    public async Task UpdateTenant_NameAndCode()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var newName = TestHelpers.GetRandomString();
        var newCode = TestHelpers.GetRandomString();
        await _dbLocator.UpdateTenant(tenantId, newName, newCode);

        var updatedTenant = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(newName, updatedTenant.Name);
        Assert.Equal(newCode, updatedTenant.Code);
        Assert.Equal(Status.Active, updatedTenant.Status);
    }

    [Fact]
    public async Task CreateTenantWithNameAndStatus()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, Status.Active);

        var tenant = await _dbLocator.GetTenant(tenantId);
        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Null(tenant.Code);
        Assert.Equal(Status.Active, tenant.Status);
    }

    [Fact]
    public async Task CreateTenantButTenantNameAlreadyExists()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active)
        );
    }

    [Fact]
    public async Task GetTenants_ReturnsCachedData()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantCode = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, tenantCode, Status.Active);

        var tenants = await _dbLocator.GetTenants();
        Assert.Contains(tenants, t => t.Id == tenantId);

        var cachedTenants = await _dbLocator.GetTenants();

        Assert.NotNull(cachedTenants);
        Assert.Contains(cachedTenants, t => t.Id == tenantId);
        Assert.Contains(cachedTenants, t => t.Name == tenantName);
        Assert.Contains(cachedTenants, t => t.Code == tenantCode);
        Assert.Contains(cachedTenants, t => t.Status == Status.Active);

        var cacheKey = "tenants";
        var cachedData = await _cache.GetCachedData<List<Tenant>>(cacheKey);
        Assert.NotNull(cachedData);
        Assert.Equal(cachedTenants.Count, cachedData.Count);
        Assert.Equal(cachedTenants[0].Id, cachedData[0].Id);
        Assert.Equal(cachedTenants[0].Name, cachedData[0].Name);
        Assert.Equal(cachedTenants[0].Code, cachedData[0].Code);
        Assert.Equal(cachedTenants[0].Status, cachedData[0].Status);
    }
}
