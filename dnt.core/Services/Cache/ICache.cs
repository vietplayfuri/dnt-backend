namespace dnt.core.Services.Cache
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Use this interface to interact with the cache. Do not use IMemoryCache
    /// </summary>
    public interface ICache
    {
        bool TryGetValue<T>(object key, out T value);
        Task<bool> TryGetValueAsync<T>(object key, out T value);

        void Set(object key, object value, CacheEntryOptions options = null);
        Task SetAsync(object key, object value, CacheEntryOptions options = null);

        void Remove(object key);
        Task RemoveAsync(object key);
    }
}