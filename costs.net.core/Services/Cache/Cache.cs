namespace dnt.core.Services.Cache
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Models.Utils;

    public class Cache : ICache
    {
        private readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromDays(1);
        private readonly TimeSpan _cacheExpiration;

        private readonly IMemoryCache _memoryCache;

        public Cache(IOptions<CacheSettings> cacheSettings, IMemoryCache memoryCache)
        {
            var settings = cacheSettings?.Value ?? throw new ArgumentNullException(nameof(cacheSettings));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            if (!TimeSpan.TryParseExact(settings.DefaultExpiration, "G", CultureInfo.CurrentCulture, out _cacheExpiration))
            {
                _cacheExpiration = _defaultCacheExpiration;
            }
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public Task<bool> TryGetValueAsync<T>(object key, out T value)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out value));
        }

        public void Set(object key, object value, CacheEntryOptions options = null)
        {
            var cacheEntryOptions = GetMemoryCacheEntryOptions(options);

            _memoryCache.Set(key, value, cacheEntryOptions);
        }

        public Task SetAsync(object key, object value, CacheEntryOptions options = null)
        {
            Set(key, value, options);

            return Task.CompletedTask;
        }

        public void Remove(object key)
        {
            _memoryCache.Remove(key);
        }

        public Task RemoveAsync(object key)
        {
            _memoryCache.Remove(key);

            return Task.CompletedTask;
        }

        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions(CacheEntryOptions options)
        {
            if (options?.Expiration == null)
            {
                return new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(_cacheExpiration);
            }

            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(options.Expiration.Value);
        }
    }
}