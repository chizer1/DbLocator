using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

internal class GetTenantsQuery { }

internal sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    internal GetTenantsQueryValidator() { }
}

internal class GetTenants(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<List<Tenant>> Handle(GetTenantsQuery query)
    {
        await new GetTenantsQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "tenants";
        var cachedData = await cache?.GetCachedData<List<Tenant>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var tenants = await GetTenantsFromDatabase(dbContextFactory);
        await cache?.CacheData(cacheKey, tenants);

        return tenants;
    }

    private static async Task<List<Tenant>> GetTenantsFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenantEntities = await dbContext.Set<TenantEntity>().ToListAsync();

        var tenants = tenantEntities
            .Select(entity => new Tenant(
                entity.TenantId,
                entity.TenantName,
                entity.TenantCode,
                (Status)entity.TenantStatusId
            ))
            .ToList();

        return tenants;
    }
}
