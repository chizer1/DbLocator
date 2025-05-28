using DbLocator.Domain;

namespace DbLocator.Services.Tenant;

internal interface ITenantService
{
    Task<int> AddTenant(string tenantName);
    Task<int> AddTenant(string tenantName, Status tenantStatus);
    Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus);
    Task DeleteTenant(int tenantId);
    Task<List<Domain.Tenant>> GetTenants();
    Task<Domain.Tenant> GetTenant(int tenantId);
    Task<Domain.Tenant> GetTenant(string tenantCode);
    Task UpdateTenant(int tenantId, Status tenantStatus);
    Task UpdateTenant(int tenantId, string tenantName);
    Task UpdateTenant(int tenantId, string tenantName, string tenantCode);
}
