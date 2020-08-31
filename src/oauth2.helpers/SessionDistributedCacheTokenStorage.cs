using InMemoryIdentityApp.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public class SessionDistributedCacheTokenStorage : DistributedCacheTokenStorage<SessionDistributedCacheTokenStorage>
    {
        public static string GuidS => Guid.NewGuid().ToString();
        const string _sessionKey = "f84e517c-df38-4cfa-b8cb-92e65d962887";
        private IHttpContextAccessor _httpContextAccessor;

        public class SessionKey
        {
            public string Key { get; set; }
        }
     
        public SessionDistributedCacheTokenStorage(
            IDistributedCache cache,
            IHttpContextAccessor httpContextAccessor, 
            ILogger<SessionDistributedCacheTokenStorage> logger) : base(cache, logger)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async override Task<string> GetCacheKeyAsync()
        {
            var sessionKey = _httpContextAccessor.HttpContext.Session.Get<SessionKey>(_sessionKey);
            if (sessionKey == null)
            {
                sessionKey = new SessionKey
                {
                    Key = GuidS
                };
                _httpContextAccessor.HttpContext.Session.Set(_sessionKey, sessionKey);
            }
            return sessionKey.Key;
        }
    }
}
