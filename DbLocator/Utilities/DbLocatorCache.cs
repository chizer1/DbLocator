using System.Text.Json;
using DbLocator.Domain;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Utilities;

internal class DbLocatorCache(IDistributedCache cache)
{
    internal async Task<T> GetCachedData<T>(string cacheKey)
    {
        if (cache == null)
        {
            return default;
        }

        var cachedData = cache != null ? await cache.GetStringAsync(cacheKey) : null;
        return cachedData != null ? JsonSerializer.Deserialize<T>(cachedData) : default;
    }

    internal async Task CacheData(string cacheKey, object data)
    {
        if (cache == null)
        {
            return;
        }

        var serializedData = JsonSerializer.Serialize(data);
        await cache.SetStringAsync(cacheKey, serializedData);
    }

    internal async Task Remove(string cacheKey)
    {
        if (cache == null)
        {
            return;
        }

        await cache.RemoveAsync(cacheKey);
    }

    internal async Task CacheConnectionString(string cacheKey, string connectionString)
    {
        if (cache == null)
        {
            return;
        }

        await cache.SetStringAsync(cacheKey, connectionString);

        // Add cacheKey to cached dictionary
        var cacheKeys = await GetCachedData<List<string>>("connectionCacheKeys") ?? [];
        cacheKeys.Add(cacheKey);
        await CacheData("connectionCacheKeys", cacheKeys);
    }

    // Note, if this is ran with no specified parameters,
    // it will clear all connection strings from the cache
    internal async Task TryClearConnectionStringFromCache(
        int? TenantId = null,
        int? DatabaseTypeId = null,
        int? ConnectionId = null,
        string TenantCode = null,
        DatabaseRole[] Roles = null
    )
    {
        if (cache == null)
        {
            return;
        }

        var cacheKeys = await GetCachedData<List<string>>("connectionCacheKeys") ?? [];
        foreach (var cacheKey in cacheKeys)
        {
            if (TenantId != null && !cacheKey.Contains($"TenantId:{TenantId}"))
            {
                continue;
            }

            if (DatabaseTypeId != null && !cacheKey.Contains($"DatabaseTypeId:{DatabaseTypeId}"))
            {
                continue;
            }

            if (ConnectionId != null && !cacheKey.Contains($"ConnectionId:{ConnectionId}"))
            {
                continue;
            }

            if (TenantCode != null && !cacheKey.Contains($"TenantCode:{TenantCode}"))
            {
                continue;
            }

            if (Roles != null && !cacheKey.Contains($"Roles:{string.Join(",", Roles)}"))
            {
                continue;
            }

            await cache.RemoveAsync(cacheKey);
        }
    }
}
