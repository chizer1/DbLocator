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

        var cachedData = await cache.GetAsync(cacheKey);
        if (cachedData != null)
        {
            Console.WriteLine("Cache hit!");
            try
            {
                var json = Encoding.UTF8.GetString(cachedData);
                Console.WriteLine($"Cache data: {json}");

                var tenantEntities = JsonSerializer.Deserialize<List<TenantEntity>>(
                    json,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                if (tenantEntities != null)
                    return MapToTenants(tenantEntities);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Cache deserialization failed: {ex.Message}");
            }
        }

        // Fetch from database if cache miss
        Console.WriteLine("Cache miss. Fetching from DB...");
        var tenantEntitiesFromDb = await dbContext.Set<TenantEntity>().ToListAsync();

        if (tenantEntitiesFromDb.Count > 0)
        {
            var serializedJson = JsonSerializer.Serialize(
                tenantEntitiesFromDb,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }
            );
            Console.WriteLine($"Storing in cache: {serializedJson}");

            await cache.SetAsync(
                cacheKey,
                Encoding.UTF8.GetBytes(serializedJson),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
                }
            );
        }

        return MapToTenants(tenantEntitiesFromDb);
    }

    private static List<Tenant> MapToTenants(List<TenantEntity> tenantEntities)
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
