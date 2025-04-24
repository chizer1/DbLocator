namespace DbLocator.Utilities;

/// <summary>
/// Interface for caching data in DbLocator.
/// </summary>
public interface IDbLocatorCache
{
    /// <summary>
    /// Gets cached data of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve.</typeparam>
    /// <param name="cacheKey">The key to identify the cached data.</param>
    /// <returns>The cached data, or default if not found.</returns>
    Task<T> GetCachedData<T>(string cacheKey);

    /// <summary>
    /// Caches the specified data.
    /// </summary>
    /// <param name="cacheKey">The key to identify the cached data.</param>
    /// <param name="data">The data to cache.</param>
    Task CacheData(string cacheKey, object data);

    /// <summary>
    /// Removes the specified data from the cache.
    /// </summary>
    /// <param name="cacheKey">The key to identify the cached data.</param>
    Task Remove(string cacheKey);
}
