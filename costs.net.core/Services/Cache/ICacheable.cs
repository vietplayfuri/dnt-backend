namespace dnt.core.Services.Cache
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Cache;

    /// <summary>
    ///     This interface should be implemented by any service that stores data in the cache. It is used to reset the cache
    /// </summary>
    public interface ICacheable
    {
        CacheEntryType CacheEntryType { get; }

        Task<IEnumerable<string>> GetKeysAsync();

        Task RemoveAllAsync();
    }
}
