#nullable enable

using System.Text.Json;
using DbLocator.Domain;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Utilities;

/// <summary>
/// Provides caching functionality for DbLocator entities and connection strings.
/// </summary>
internal class DbLocatorCache(IDistributedCache? cache)
{
    private readonly IDistributedCache? _cache = cache;
    private const string ConnectionCacheKeysKey = "connectionCacheKeys";

    /// <summary>
    /// Retrieves cached data of type T for the specified cache key.
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve.</typeparam>
    /// <param name="cacheKey">The key used to identify the cached data.</param>
    /// <returns>The cached data if found; otherwise, default(T).</returns>
    internal async Task<T?> GetCachedData<T>(string cacheKey)
    {
        if (_cache == null)
            return default;

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData == null)
        {
            return default;
        }

        // Handle string type separately to avoid JSON deserialization
        if (typeof(T) == typeof(string))
        {
            return (T)(object)cachedData;
        }

        return JsonSerializer.Deserialize<T>(cachedData);
    }

    /// <summary>
    /// Caches the specified data with the given cache key.
    /// </summary>
    /// <param name="cacheKey">The key to use for caching the data.</param>
    /// <param name="data">The data to cache.</param>
    internal async Task CacheData(string cacheKey, object data)
    {
        if (_cache == null)
            return;

        // Handle string type separately to avoid JSON serialization
        if (data is string stringData)
        {
            await _cache.SetStringAsync(cacheKey, stringData);
            return;
        }

        var serializedData = JsonSerializer.Serialize(data);
        await _cache.SetStringAsync(cacheKey, serializedData);
    }

    /// <summary>
    /// Removes the cached data for the specified cache key.
    /// </summary>
    /// <param name="cacheKey">The key of the cached data to remove.</param>
    internal async Task Remove(string cacheKey)
    {
        if (_cache == null)
            return;

        await _cache.RemoveAsync(cacheKey);
    }

    /// <summary>
    /// Caches a connection string and adds its key to the list of connection cache keys.
    /// </summary>
    /// <param name="cacheKey">The key to use for caching the connection string.</param>
    /// <param name="connectionString">The connection string to cache.</param>
    internal async Task CacheConnectionString(string cacheKey, string connectionString)
    {
        if (_cache == null)
            return;

        await _cache.SetStringAsync(cacheKey, connectionString);

        // Add cacheKey to cached dictionary
        var cacheKeys = await GetCachedData<List<string>>(ConnectionCacheKeysKey) ?? [];
        if (!cacheKeys.Contains(cacheKey))
        {
            cacheKeys.Add(cacheKey);
            await CacheData(ConnectionCacheKeysKey, cacheKeys);
        }
    }

    /// <summary>
    /// Clears connection strings from the cache based on specified criteria.
    /// </summary>
    /// <param name="tenantId">Optional tenant ID to filter connection strings.</param>
    /// <param name="databaseTypeId">Optional database type ID to filter connection strings.</param>
    /// <param name="connectionId">Optional connection ID to filter connection strings.</param>
    /// <param name="tenantCode">Optional tenant code to filter connection strings.</param>
    /// <param name="roles">Optional database roles to filter connection strings.</param>
    internal async Task TryClearConnectionStringFromCache(
        int? tenantId = null,
        int? databaseTypeId = null,
        int? connectionId = null,
        string? tenantCode = null,
        DatabaseRole[]? roles = null
    )
    {
        if (_cache == null)
            return;

        var cacheKeys = await GetCachedData<List<string>>(ConnectionCacheKeysKey) ?? [];
        var keysToRemove = new List<string>();

        foreach (var cacheKey in cacheKeys)
        {
            if (
                ShouldRemoveCacheKey(
                    cacheKey,
                    tenantId,
                    databaseTypeId,
                    connectionId,
                    tenantCode,
                    roles
                )
            )
            {
                keysToRemove.Add(cacheKey);
            }
        }

        foreach (var key in keysToRemove)
        {
            await _cache.RemoveAsync(key);
            cacheKeys.Remove(key);
        }

        if (keysToRemove.Count != 0)
            await CacheData(ConnectionCacheKeysKey, cacheKeys);
    }

    private static bool ShouldRemoveCacheKey(
        string cacheKey,
        int? tenantId,
        int? databaseTypeId,
        int? connectionId,
        string? tenantCode,
        DatabaseRole[]? roles
    )
    {
        if (tenantId != null && !cacheKey.Contains($"TenantId:{tenantId}"))
            return false;

        if (databaseTypeId != null && !cacheKey.Contains($"DatabaseTypeId:{databaseTypeId}"))
            return false;

        if (connectionId != null && !cacheKey.Contains($"ConnectionId:{connectionId}"))
            return false;

        if (tenantCode != null && !cacheKey.Contains($"TenantCode:{tenantCode}"))
            return false;

        if (roles != null)
        {
            var rolesString = string.Join(",", roles);
            if (!cacheKey.Contains($"Roles:{rolesString}"))
                return false;
        }

        return true;
    }
}
