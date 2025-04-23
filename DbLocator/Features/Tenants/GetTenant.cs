using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

internal class GetTenantQuery
{
    public int TenantId { get; set; }
}

internal sealed class GetTenantQueryValidator : AbstractValidator<GetTenantQuery>
{
    internal GetTenantQueryValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0);
    }
}

internal class GetTenant(IDbContextFactory<DbLocatorContext> dbContextFactory, DbLocatorCache cache)
{
    internal async Task<Tenant> Handle(GetTenantQuery query)
    {
        await new GetTenantQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = $"tenant-{query.TenantId}";
        var cachedData = await cache?.GetCachedData<Tenant>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var tenant = await GetTenantFromDatabase(dbContextFactory, query.TenantId);
        await cache?.CacheData(cacheKey, tenant);

        return tenant;
    }

    private static async Task<Tenant> GetTenantFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        int tenantId
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenantEntity = await dbContext
            .Set<TenantEntity>()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);

        if (tenantEntity == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");
        }

        return new Tenant(
            tenantEntity.TenantId,
            tenantEntity.TenantName,
            tenantEntity.TenantCode,
            (Status)tenantEntity.TenantStatusId
        );
    }
}
