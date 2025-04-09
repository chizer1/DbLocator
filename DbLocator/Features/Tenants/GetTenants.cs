using System.Text;
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
        var cachedData = await GetCachedData(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return DeserializeCachedData(cachedData);

        var tenants = await GetTenantsFromDatabase(dbContextFactory);
        await CacheData(cacheKey, tenants);

        return tenants;
    }

    private async Task<string> GetCachedData(string cacheKey)
    {
        return cache != null ? await cache.GetCachedData<string>(cacheKey) : null;
    }

    private static List<Tenant> DeserializeCachedData(string cachedData)
    {
        return JsonSerializer.Deserialize<List<Tenant>>(cachedData) ?? new List<Tenant>();
    }

    private async Task CacheData(string cacheKey, List<Tenant> tenants)
    {
        if (cache != null)
        {
            var serializedData = JsonSerializer.Serialize(tenants);
            await cache.CacheData(cacheKey, serializedData);
        }
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
