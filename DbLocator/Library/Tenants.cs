using DbLocator.Domain;
using DbLocator.Features.Tenants;
using DbLocator.Features.Tenants.AddTenant;
using DbLocator.Features.Tenants.DeleteTenant;
using DbLocator.Features.Tenants.GetTenants;
using DbLocator.Features.Tenants.UpdateTenant;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class Tenants
{
    private readonly AddTenant _addTenant;
    private readonly GetTenants _getTenants;
    private readonly UpdateTenant _updateTenant;
    private readonly DeleteTenant _deleteTenant;

    public Tenants(DbContext dbContext)
    {
        ITenantRepository tenantRepository = new TenantRepository(dbContext);

        _addTenant = new AddTenant(tenantRepository);
        _getTenants = new GetTenants(tenantRepository);
        _updateTenant = new UpdateTenant(tenantRepository);
        _deleteTenant = new DeleteTenant(tenantRepository);
    }

    public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _addTenant.Handle(new AddTenantCommand(tenantName, tenantCode, tenantStatus));
    }

    public async Task<List<Tenant>> GetTenants()
    {
        return await _getTenants.Handle(new GetTenantsQuery());
    }

    public async Task UpdateTenant(
        int tenantId,
        string tenantName,
        string tenantCode,
        Status tenantStatus
    )
    {
        await _updateTenant.Handle(
            new UpdateTenantCommand(tenantId, tenantName, tenantCode, tenantStatus)
        );
    }

    public async Task DeleteTenant(int tenantId)
    {
        await _deleteTenant.Handle(new DeleteTenantCommand(tenantId));
    }
}
