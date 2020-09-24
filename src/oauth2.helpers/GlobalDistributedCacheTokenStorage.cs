using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public class GlobalDistributedCacheTokenStorage : DistributedCacheTokenStorage<GlobalDistributedCacheTokenStorage>
    {
        const string _cacheKey = "jmu35eC/BBbtV7QODlFVD4QZLR2vUP73Z3vI/1BAQug=.CsBu1d6kaefFmIitghoTyw==";

        public GlobalDistributedCacheTokenStorage(ISerializer serializer, ISymmetricEncryptor encryptor, IDistributedCache cache, ILogger<GlobalDistributedCacheTokenStorage> logger) : base(serializer, encryptor, cache, logger)
        {
        }

        public override Task<string> GetCacheKeyAsync()
        {
            return Task.FromResult(_cacheKey);
        }
    }
}
