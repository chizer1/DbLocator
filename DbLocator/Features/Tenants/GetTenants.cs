using System.Text;
using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.Tenants;

internal class GetTenantsQuery { }

internal sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    internal GetTenantsQueryValidator() { }
}

internal class GetTenants(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task<List<Tenant>> Handle(GetTenantsQuery query)
    {
        await new GetTenantsQueryValidator().ValidateAndThrowAsync(query);
        const string cacheKey = "tenants";

        var cachedData = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
            return JsonSerializer.Deserialize<List<Tenant>>(cachedData);

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

        var serializedData = JsonSerializer.Serialize(tenants);
        await cache.SetStringAsync(cacheKey, serializedData);

        return tenants;
    }
}
