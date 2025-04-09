using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Utilities;

internal class DbLocatorCache(IDistributedCache cache)
{
    private readonly IDistributedCache _cache = cache;

    internal async Task<T> GetCachedData<T>(string cacheKey)
    {
        if (_cache == null)
        {
            return default;
        }

        var cachedData = _cache != null ? await _cache.GetStringAsync(cacheKey) : null;
        return cachedData != null ? JsonSerializer.Deserialize<T>(cachedData) : default;
    }

    internal async Task CacheData(string cacheKey, object data)
    {
        if (_cache == null)
        {
            return;
        }

        var serializedData = JsonSerializer.Serialize(data);
        await _cache.SetStringAsync(cacheKey, serializedData);
    }

    internal async Task CacheConnectionString(string cacheKey, string connectionString)
    {
        if (_cache == null)
        {
            return;
        }

        await _cache.SetStringAsync(cacheKey, connectionString);

        // Add cacheKey to cached dictionary
        var cacheKeys = await GetCachedData<List<string>>("connectionCacheKeys") ?? [];
        cacheKeys.Add(cacheKey);
        await CacheData("connectionCacheKeys", cacheKeys);
    }

    internal async Task Remove(string cacheKey)
    {
        if (_cache == null)
        {
            return;
        }

        await _cache.RemoveAsync(cacheKey);
    }

    internal async Task TryClearConnectionStringFromCache(string connectionCacheKeyPiece)
    {
        if (_cache == null)
        {
            return;
        }

        var cacheKeys = await GetCachedData<List<string>>("connectionCacheKeys") ?? [];
        foreach (var cacheKey in cacheKeys)
        {
            if (cacheKey.Contains(connectionCacheKeyPiece))
            {
                await _cache.RemoveAsync(cacheKey);
            }
        }
    }
}
