namespace dnt.core.Services.Cache
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Cache;

    public interface ICacheService
    {
        Task<List<string>> GetAllKeys(CacheEntryType? type);
        Task RemoveCache(CacheEntryType? type);
    }
}
