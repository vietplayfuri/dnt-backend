namespace dnt.core.Services.Cache
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Cache;

    public class CacheService : ICacheService
    {
        private readonly IEnumerable<ICacheable> _cacheables;

        public CacheService(IEnumerable<ICacheable> cacheables)
        {
            _cacheables = cacheables;
        }

        public async Task<List<string>> GetAllKeys(CacheEntryType? type)
        {
            var keys = new List<string>();
            foreach (var cacheable in _cacheables)
            {
                if (!type.HasValue || cacheable.CacheEntryType == type.Value)
                {
                    keys.AddRange(await cacheable.GetKeysAsync());
                }
            }

            return keys;
        }

        public async Task RemoveCache(CacheEntryType? type)
        {
            foreach (var cacheable in _cacheables)
            {
                if (!type.HasValue || cacheable.CacheEntryType == type.Value)
                {
                    await cacheable.RemoveAllAsync();
                }
            }
        }
    }
}
