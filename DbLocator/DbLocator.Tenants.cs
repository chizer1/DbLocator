using DbLocator.Domain;

namespace DbLocator;

public partial class Locator
{
    /// <summary>
    ///Add tenant
    /// </summary>
    /// <param name="tenantName"></param>
    /// <param name="tenantCode"></param>
    /// <param name="tenantStatus"></param>
    /// <returns>TenantId</returns>
    public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _tenants.AddTenant(tenantName, tenantCode, tenantStatus);
    }

    /// <summary>
    ///Add tenant
    /// </summary>
    /// <param name="tenantName"></param>
    /// <param name="tenantStatus"></param>
    /// <returns>TenantId</returns>
    public async Task<int> AddTenant(string tenantName, Status tenantStatus)
    {
        return await _tenants.AddTenant(tenantName, tenantStatus);
    }

    /// <summary>
    ///Add Tenant
    /// </summary>
    /// <returns>TenantId</returns>
    /// <param name="tenantName"></param>
    public async Task<int> AddTenant(string tenantName)
    {
        return await _tenants.AddTenant(tenantName);
    }

    /// <summary>
    ///Get tenants
    /// </summary>
    /// <returns>List of tenants</returns>
    public async Task<List<Tenant>> GetTenants()
    {
        return await _tenants.GetTenants();
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantName"></param>
    /// <param name="tenantCode"></param>
    /// <param name="tenantStatus"></param>
    /// <returns></returns>
    public async Task UpdateTenant(
        int tenantId,
        string tenantName,
        string tenantCode,
        Status tenantStatus
    )
    {
        await _tenants.UpdateTenant(tenantId, tenantName, tenantCode, tenantStatus);
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantName"></param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, string tenantName)
    {
        await _tenants.UpdateTenant(tenantId, tenantName);
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantStatus"></param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, Status tenantStatus)
    {
        await _tenants.UpdateTenant(tenantId, tenantStatus);
    }

    /// <summary>
    ///Update tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="tenantName"></param>
    /// <param name="tenantCode"></param>
    /// <returns></returns>
    public async Task UpdateTenant(int tenantId, string tenantName, string tenantCode)
    {
        await _tenants.UpdateTenant(tenantId, tenantName, tenantCode);
    }

    /// <summary>
    ///Delete tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public async Task DeleteTenant(int tenantId)
    {
        await _tenants.DeleteTenant(tenantId);
    }
}
