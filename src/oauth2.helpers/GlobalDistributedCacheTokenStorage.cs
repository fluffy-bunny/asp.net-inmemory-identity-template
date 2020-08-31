using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public class GlobalDistributedCacheTokenStorage : DistributedCacheTokenStorage<GlobalDistributedCacheTokenStorage>
    {
        const string _cacheKey = "2d452551-0686-4221-8ad4-5786d7d8f2a2";
        public GlobalDistributedCacheTokenStorage(IDistributedCache cache, ILogger<GlobalDistributedCacheTokenStorage> logger) : base(cache, logger)
        {
        }
        public override Task<string> GetCacheKeyAsync()
        {
            return Task.FromResult(_cacheKey);
        }
    }
}
