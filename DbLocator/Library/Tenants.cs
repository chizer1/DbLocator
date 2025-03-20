using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Library;

internal class Tenants(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    private readonly AddTenant _addTenant = new(dbContextFactory, cache);
    private readonly DeleteTenant _deleteTenant = new(dbContextFactory, cache);
    private readonly GetTenants _getTenants = new(dbContextFactory, cache);
    private readonly UpdateTenant _updateTenant = new(dbContextFactory, cache);

    internal async Task<int> AddTenant(string tenantName)
    {
        return await _addTenant.Handle(new AddTenantCommand(tenantName, null, Status.Active));
    }

    internal async Task<int> AddTenant(string tenantName, Status tenantStatus)
    {
        return await _addTenant.Handle(new AddTenantCommand(tenantName, null, tenantStatus));
    }

    internal async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _addTenant.Handle(new AddTenantCommand(tenantName, tenantCode, tenantStatus));
    }

    internal async Task DeleteTenant(int tenantId)
    {
        await _deleteTenant.Handle(new DeleteTenantCommand(tenantId));
    }

    internal async Task<List<Tenant>> GetTenants()
    {
        return await _getTenants.Handle(new GetTenantsQuery());
    }

    internal async Task UpdateTenant(int tenantId, Status tenantStatus)
    {
        await _updateTenant.Handle(new UpdateTenantCommand(tenantId, null, null, tenantStatus));
    }

    internal async Task UpdateTenant(int tenantId, string tenantName)
    {
        await _updateTenant.Handle(new UpdateTenantCommand(tenantId, tenantName, null, null));
    }

    internal async Task UpdateTenant(int tenantId, string tenantName, string tenantCode)
    {
        await _updateTenant.Handle(new UpdateTenantCommand(tenantId, tenantName, tenantCode, null));
    }

    internal async Task UpdateTenant(
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
}
