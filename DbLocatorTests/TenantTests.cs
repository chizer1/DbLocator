using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class TenantTests : IAsyncLifetime
{
    private readonly Locator _dbLocator;
    private readonly DbLocatorCache _cache;
    private readonly List<Tenant> _testTenants = new();

    public TenantTests(DbLocatorFixture dbLocatorFixture)
    {
        _dbLocator = dbLocatorFixture.DbLocator;
        _cache = dbLocatorFixture.LocatorCache;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var tenant in _testTenants)
        {
            try
            {
                await _dbLocator.DeleteTenant(tenant.Id);
            }
            catch { }
        }
        _testTenants.Clear();
        await _cache.Remove("tenants");
    }

    private async Task<Tenant> CreateTenantAsync(
        string name = null,
        string code = null,
        Status status = Status.Active)
    {
        name ??= TestHelpers.GetRandomString();
        code ??= TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(name, code, status);
        var tenant = await _dbLocator.GetTenant(tenantId);
        _testTenants.Add(tenant);
        return tenant;
    }

    #region Creation Tests
    [Fact]
    public async Task CreateMultipleTenantsAndSearchByKeyWord()
    {
        var tenant1 = await CreateTenantAsync();
        var tenant2 = await CreateTenantAsync();

        var tenants = await _dbLocator.GetTenants();
        Assert.Contains(tenants, t => t.Id == tenant1.Id);
        Assert.Contains(tenants, t => t.Id == tenant2.Id);
    }

    [Fact]
    public async Task CreateTenantWithOnlyName()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName);
        var tenant = await _dbLocator.GetTenant(tenantId);
        _testTenants.Add(tenant);

        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Null(tenant.Code);
        Assert.Equal(Status.Active, tenant.Status);
    }

    [Fact]
    public async Task CreateTenantWithNameAndStatus()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.CreateTenant(tenantName, Status.Inactive);
        var tenant = await _dbLocator.GetTenant(tenantId);
        _testTenants.Add(tenant);

        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Null(tenant.Code);
        Assert.Equal(Status.Inactive, tenant.Status);
    }

    [Fact]
    public async Task CreateTenantButTenantNameAlreadyExists()
    {
        var tenant = await CreateTenantAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.CreateTenant(tenant.Name)
        );
    }
    #endregion

    #region Cache Tests
    [Fact]
    public async Task VerifyTenantsAreCached()
    {
        var tenant = await CreateTenantAsync();

        var tenants = await _dbLocator.GetTenants();
        Assert.Contains(tenants, t => t.Id == tenant.Id);

        var cachedTenants = await _cache.GetCachedData<List<Tenant>>("tenants");
        Assert.NotNull(cachedTenants);
        Assert.Contains(cachedTenants, t => t.Id == tenant.Id);
    }

    [Fact]
    public async Task GetTenantById_ReturnsCachedData()
    {
        var tenant = await CreateTenantAsync();

        // First call should populate cache
        var firstCall = await _dbLocator.GetTenant(tenant.Id);
        Assert.NotNull(firstCall);

        // Delete the tenant to ensure we're getting from cache
        await _dbLocator.DeleteTenant(tenant.Id);

        // Second call should use cache
        var secondCall = await _dbLocator.GetTenant(tenant.Id);
        Assert.NotNull(secondCall);
        Assert.Equal(tenant.Id, secondCall.Id);
        Assert.Equal(tenant.Name, secondCall.Name);
        Assert.Equal(tenant.Code, secondCall.Code);
    }

    [Fact]
    public async Task GetTenantByCode_ReturnsCachedData()
    {
        var tenant = await CreateTenantAsync();

        // First call should populate cache
        var firstCall = await _dbLocator.GetTenant(tenant.Code);
        Assert.NotNull(firstCall);

        // Delete the tenant to ensure we're getting from cache
        await _dbLocator.DeleteTenant(tenant.Id);

        // Second call should use cache
        var secondCall = await _dbLocator.GetTenant(tenant.Code);
        Assert.NotNull(secondCall);
        Assert.Equal(tenant.Id, secondCall.Id);
        Assert.Equal(tenant.Name, secondCall.Name);
        Assert.Equal(tenant.Code, secondCall.Code);
    }
    #endregion

    #region Update Tests
    [Fact]
    public async Task UpdateTenant_WithAllProperties()
    {
        var tenant = await CreateTenantAsync();
        var newName = TestHelpers.GetRandomString();
        var newCode = TestHelpers.GetRandomString();

        await _dbLocator.UpdateTenant(tenant.Id, newName, newCode);
        await _dbLocator.UpdateTenant(tenant.Id, Status.Inactive);

        var updatedTenant = await _dbLocator.GetTenant(tenant.Id);
        Assert.Equal(newName, updatedTenant.Name);
        Assert.Equal(newCode, updatedTenant.Code);
        Assert.Equal(Status.Inactive, updatedTenant.Status);
    }

    [Fact]
    public async Task UpdateTenant_StatusOnly()
    {
        var tenant = await CreateTenantAsync();

        await _dbLocator.UpdateTenant(tenant.Id, Status.Inactive);

        var updatedTenant = await _dbLocator.GetTenant(tenant.Id);
        Assert.Equal(Status.Inactive, updatedTenant.Status);
        Assert.Equal(tenant.Name, updatedTenant.Name);
        Assert.Equal(tenant.Code, updatedTenant.Code);
    }

    [Fact]
    public async Task UpdateTenant_NameOnly()
    {
        var tenant = await CreateTenantAsync();
        var newName = TestHelpers.GetRandomString();

        await _dbLocator.UpdateTenant(tenant.Id, newName);

        var updatedTenant = await _dbLocator.GetTenant(tenant.Id);
        Assert.Equal(newName, updatedTenant.Name);
        Assert.Equal(tenant.Code, updatedTenant.Code);
        Assert.Equal(Status.Active, updatedTenant.Status);
    }

    [Fact]
    public async Task UpdateTenant_NameAndCode()
    {
        var tenant = await CreateTenantAsync();
        var newName = TestHelpers.GetRandomString();
        var newCode = TestHelpers.GetRandomString();

        await _dbLocator.UpdateTenant(tenant.Id, newName, newCode);

        var updatedTenant = await _dbLocator.GetTenant(tenant.Id);
        Assert.Equal(newName, updatedTenant.Name);
        Assert.Equal(newCode, updatedTenant.Code);
        Assert.Equal(Status.Active, updatedTenant.Status);
    }

    [Fact]
    public async Task UpdateTenant_ToDuplicateName_ThrowsInvalidOperationException()
    {
        var tenant1 = await CreateTenantAsync();
        var tenant2 = await CreateTenantAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.UpdateTenant(tenant2.Id, tenant1.Name)
        );
    }

    [Fact]
    public async Task UpdateTenant_ToDuplicateCode_ThrowsInvalidOperationException()
    {
        var tenant1 = await CreateTenantAsync();
        var tenant2 = await CreateTenantAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.UpdateTenant(tenant2.Id, null, tenant1.Code)
        );
    }
    #endregion

    #region Delete Tests
    [Fact]
    public async Task DeleteTenant()
    {
        var tenant = await CreateTenantAsync();
        await _dbLocator.DeleteTenant(tenant.Id);

        var tenants = await _dbLocator.GetTenants();
        Assert.DoesNotContain(tenants, t => t.Id == tenant.Id);
    }

    [Fact]
    public async Task CannotDeleteTenantWithActiveConnections()
    {
        var tenant = await CreateTenantAsync();

        var dbName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(dbName, 1, 1, false);
        await _dbLocator.CreateConnection(tenant.Id, databaseId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteTenant(tenant.Id)
        );
    }
    #endregion

    #region Validation Tests
    [Fact]
    public async Task GetNonExistentTenantById_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.GetTenant(999999)
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
    #endregion
}
