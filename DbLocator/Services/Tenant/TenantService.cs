using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Tenants.CreateTenant;
using DbLocator.Features.Tenants.DeleteTenant;
using DbLocator.Features.Tenants.GetTenantByCode;
using DbLocator.Features.Tenants.GetTenantById;
using DbLocator.Features.Tenants.GetTenants;
using DbLocator.Features.Tenants.UpdateTenant;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.Tenant;

internal class TenantService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : ITenantService
{
    private readonly CreateTenantHandler _createTenant = new(dbContextFactory, cache);
    private readonly DeleteTenantHandler _deleteTenant = new(dbContextFactory, cache);
    private readonly GetTenantsHandler _getTenants = new(dbContextFactory, cache);
    private readonly GetTenantByIdHandler _getTenantById = new(dbContextFactory, cache);
    private readonly GetTenantByCodeHandler _getTenantByCode = new(dbContextFactory, cache);
    private readonly UpdateTenantHandler _updateTenant = new(dbContextFactory, cache);

    public async Task<int> CreateTenant(string tenantName)
    {
        return await _createTenant.Handle(new CreateTenantCommand(tenantName, null, Status.Active));
    }

    public async Task<int> CreateTenant(string tenantName, Status tenantStatus)
    {
        return await _createTenant.Handle(new CreateTenantCommand(tenantName, null, tenantStatus));
    }

    public async Task<int> CreateTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _createTenant.Handle(
            new CreateTenantCommand(tenantName, tenantCode, tenantStatus)
        );
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
        return await _getTenantById.Handle(new GetTenantByIdQuery(tenantId));
    }

    public async Task<Domain.Tenant> GetTenant(string tenantCode)
    {
        return await _getTenantByCode.Handle(new GetTenantByCodeQuery(tenantCode));
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
}
