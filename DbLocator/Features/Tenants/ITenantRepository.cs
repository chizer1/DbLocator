using DbLocator.Domain;

namespace DbLocator.Features.Tenants;

internal interface ITenantRepository
{
    public Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus);
    public Task<Tenant> GetTenant(int tenantId);
    public Task<List<Tenant>> GetTenants();
    public Task UpdateTenant(
        int tenantId,
        string tenantName,
        string tenantCode,
        Status tenantStatus
    );
    public Task DeleteTenant(int tenantId);
}
