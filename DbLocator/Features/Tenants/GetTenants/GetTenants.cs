#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants.GetTenants;

internal record GetTenantsQuery;

internal sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    internal GetTenantsQueryValidator() { }
}

internal class GetTenantsHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<List<Tenant>> Handle(
        GetTenantsQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetTenantsQueryValidator().ValidateAndThrowAsync(request, cancellationToken);

        const string cacheKey = "tenants";
        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<List<Tenant>>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var tenantEntities = await dbContext
            .Set<TenantEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var tenants = tenantEntities
            .Select(entity => new Tenant(
                entity.TenantId,
                entity.TenantName,
                entity.TenantCode,
                (Status)entity.TenantStatusId
            ))
            .ToList();

        if (_cache != null)
        {
            await _cache.CacheData(cacheKey, tenants);
            foreach (var tenant in tenants)
            {
                await _cache.CacheData($"tenant-id-{tenant.Id}", tenant);
                if (tenant.Code != null)
                {
                    await _cache.CacheData($"tenant-code-{tenant.Code}", tenant);
                }
            }
        }

        return tenants;
    }
}
