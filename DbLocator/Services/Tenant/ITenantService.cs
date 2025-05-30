using DbLocator.Domain;

namespace DbLocator.Services.Tenant;

internal interface ITenantService
{
    Task<int> CreateTenant(string tenantName);
    Task<int> CreateTenant(string tenantName, Status tenantStatus);
    Task<int> CreateTenant(string tenantName, string tenantCode, Status tenantStatus);
    Task DeleteTenant(int tenantId);
    Task<List<Domain.Tenant>> GetTenants();
    Task<Domain.Tenant> GetTenant(int tenantId);
    Task<Domain.Tenant> GetTenant(string tenantCode);
    Task UpdateTenant(int tenantId, Status tenantStatus);
    Task UpdateTenant(int tenantId, string tenantName);
    Task UpdateTenant(int tenantId, string tenantName, string tenantCode);
    Task UpdateTenant(int tenantId, string tenantName, string tenantCode, Status tenantStatus);
}
