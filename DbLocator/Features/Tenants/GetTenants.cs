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
        await using var dbContext = dbContextFactory.CreateDbContext();
        const string cacheKey = "tenants";

        if (cache != null)
        {
            var cachedData = await cache.GetAsync(cacheKey);
            if (cachedData != null)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(cachedData);

                    var tenantEntities = JsonSerializer.Deserialize<List<TenantEntity>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }
                    );

                    if (tenantEntities != null)
                        return Mapper(tenantEntities);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Cache deserialization failed: {ex.Message}");
                }
            }

            var tenantEntitiesFromDb = await dbContext.Set<TenantEntity>().ToListAsync();
            var jsonToCache = JsonSerializer.Serialize(
                tenantEntitiesFromDb,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            await cache.SetAsync(
                cacheKey,
                Encoding.UTF8.GetBytes(jsonToCache),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                }
            );

            return Mapper(tenantEntitiesFromDb);
        }
        else
        {
            var tenantEntitiesFromDb = await dbContext.Set<TenantEntity>().ToListAsync();
            return Mapper(tenantEntitiesFromDb);
        }
    }

    private static List<Tenant> Mapper(List<TenantEntity> tenantEntities)
    {
        return
        [
            .. tenantEntities.Select(entity => new Tenant(
                entity.TenantId,
                entity.TenantName,
                entity.TenantCode,
                (Status)entity.TenantStatusId
            ))
        ];
    }
}
