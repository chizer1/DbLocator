using DbLocator.Db;
using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

internal class TenantRepository(IDbContextFactory<DbLocatorContext> dbContextFactory)
    : ITenantRepository
{
    public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenant = new TenantEntity
        {
            TenantName = tenantName,
            TenantCode = tenantCode,
            TenantStatusId = (byte)tenantStatus,
        };

        await dbContext.Set<TenantEntity>().AddAsync(tenant);
        await dbContext.SaveChangesAsync();

        return tenant.TenantId;
    }

    public async Task<Tenant> GetTenant(int tenantId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenantEntity =
            await dbContext.Set<TenantEntity>().FirstOrDefaultAsync(c => c.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        return new Tenant(
            tenantEntity.TenantId,
            tenantEntity.TenantName,
            tenantEntity.TenantCode,
            (Status)tenantEntity.TenantStatusId
        );
    }

    public async Task<List<Tenant>> GetTenants()
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenants = await dbContext.Set<TenantEntity>().ToListAsync();

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
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenant =
            await dbContext.Set<TenantEntity>().FirstOrDefaultAsync(c => c.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        tenant.TenantName = tenantName;
        tenant.TenantCode = tenantCode;
        tenant.TenantStatusId = (byte)tenantStatus;

        dbContext.Set<TenantEntity>().Update(tenant);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteTenant(int tenantId)
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenant =
            await dbContext.Set<TenantEntity>().FirstOrDefaultAsync(c => c.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        dbContext.Set<TenantEntity>().Remove(tenant);
        await dbContext.SaveChangesAsync();
    }
}
