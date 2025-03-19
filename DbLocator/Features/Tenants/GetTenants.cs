using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace DbLocator.Features.Tenants;

internal class GetTenantsQuery { }

internal sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    internal GetTenantsQueryValidator() { }
}

internal class GetTenants(IDbContextFactory<DbLocatorContext> dbContextFactory, FusionCache cache)
{
    internal async Task<List<Tenant>> Handle(GetTenantsQuery query)
    {
        await new GetTenantsQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenantEntities =
            cache != null
                ? await cache.GetOrSetAsync(
                    "tenants",
                    async _ => await dbContext.Set<TenantEntity>().ToListAsync(),
                    TimeSpan.FromHours(8)
                )
                : await dbContext.Set<TenantEntity>().ToListAsync();

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
