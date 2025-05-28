using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Tenants;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.Tenant;

internal class TenantService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : ITenantService
{
    private readonly AddTenant _addTenant = new(dbContextFactory, cache);
    private readonly DeleteTenant _deleteTenant = new(dbContextFactory, cache);
    private readonly GetTenants _getTenants = new(dbContextFactory, cache);
    private readonly GetTenant _getTenant = new(dbContextFactory, cache);
    private readonly UpdateTenant _updateTenant = new(dbContextFactory, cache);

    public async Task<int> AddTenant(string tenantName)
    {
        return await _addTenant.Handle(new AddTenantCommand(tenantName, null, Status.Active));
    }

    public async Task<int> AddTenant(string tenantName, Status tenantStatus)
    {
        return await _addTenant.Handle(new AddTenantCommand(tenantName, null, tenantStatus));
    }

    public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _addTenant.Handle(new AddTenantCommand(tenantName, tenantCode, tenantStatus));
    }

    public async Task DeleteTenant(int tenantId)
    {
        await _deleteTenant.Handle(new DeleteTenantCommand(tenantId));
    }

    public async Task<List<Domain.Tenant>> GetTenants()
    {
        return await _getTenants.Handle(new GetTenantsQuery());
    }

    public async Task<Domain.Tenant> GetTenant(int tenantId)
    {
        return await _getTenant.Handle(new GetTenantByIdQuery { TenantId = tenantId });
    }

    public async Task<Domain.Tenant> GetTenant(string tenantCode)
    {
        return await _getTenant.Handle(new GetTenantByCodeQuery { TenantCode = tenantCode });
    }

    public async Task UpdateTenant(int tenantId, Status tenantStatus)
    {
        await _updateTenant.Handle(new UpdateTenantCommand(tenantId, null, null, tenantStatus));
    }

    public async Task UpdateTenant(int tenantId, string tenantName)
    {
        await _updateTenant.Handle(new UpdateTenantCommand(tenantId, tenantName, null, null));
    }

    public async Task UpdateTenant(int tenantId, string tenantName, string tenantCode)
    {
        await _updateTenant.Handle(new UpdateTenantCommand(tenantId, tenantName, tenantCode, null));
    }
}
