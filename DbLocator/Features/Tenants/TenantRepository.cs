using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

internal class TenantRepository(DbContext DbLocatorDb) : ITenantRepository
{
    public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        var tenant = new TenantEntity
        {
            TenantName = tenantName,
            TenantCode = tenantCode,
            TenantStatusId = (byte)tenantStatus,
        };

        await DbLocatorDb.Set<TenantEntity>().AddAsync(tenant);
        await DbLocatorDb.SaveChangesAsync();

        return tenant.TenantId;
    }

    public async Task<Tenant> GetTenant(int tenantId)
    {
        var TenantEntity =
            await DbLocatorDb.Set<TenantEntity>().FirstOrDefaultAsync(c => c.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        return new Tenant(
            TenantEntity.TenantId,
            TenantEntity.TenantName,
            TenantEntity.TenantCode,
            (Status)TenantEntity.TenantStatusId
        );
    }

    public async Task<List<Tenant>> GetTenants()
    {
        var tenants = await DbLocatorDb.Set<TenantEntity>().ToListAsync();

        return tenants
            .Select(entity => new Tenant(
                entity.TenantId,
                entity.TenantName,
                entity.TenantCode,
                (Status)entity.TenantStatusId
            ))
            .ToList();
    }

    public async Task UpdateTenant(
        int tenantId,
        string tenantName,
        string tenantCode,
        Status tenantStatus
    )
    {
        var Tenant =
            await DbLocatorDb.Set<TenantEntity>().FirstOrDefaultAsync(c => c.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        Tenant.TenantName = tenantName;
        Tenant.TenantCode = tenantCode;
        Tenant.TenantStatusId = (byte)tenantStatus;

        DbLocatorDb.Set<TenantEntity>().Update(Tenant);
        await DbLocatorDb.SaveChangesAsync();
    }

    public async Task DeleteTenant(int tenantId)
    {
        var Tenant =
            await DbLocatorDb.Set<TenantEntity>().FirstOrDefaultAsync(c => c.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        DbLocatorDb.Set<TenantEntity>().Remove(Tenant);
        await DbLocatorDb.SaveChangesAsync();
    }
}
