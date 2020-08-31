using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public abstract class DistributedCacheTokenStorage<T>: TokenStorage
        where T: TokenStorage
    {

        private IDistributedCache _cache;
        private ILogger<T> _logger;
        public DistributedCacheTokenStorage(
            IDistributedCache cache, ILogger<T> logger)
        {
            _cache = cache;
            _logger = logger;

        }
        public abstract Task<string> GetCacheKeyAsync();

        protected async Task<Dictionary<string, ManagedToken>> GetManagedTokensAsync()
        {
            var cacheKey = await GetCacheKeyAsync();
            var tokens = await _cache.GetObjectFromJson<Dictionary<string, ManagedToken>>(cacheKey);
            return tokens;
        }
        protected async Task UpsertManagedTokensAsync(Dictionary<string, ManagedToken> managedTokens)
        {
            var cacheKey = await GetCacheKeyAsync();
            await _cache.SetObjectAsJson(cacheKey, managedTokens, new DistributedCacheEntryOptions
            {
                SlidingExpiration = new TimeSpan(1, 0, 0, 0)// 1 day sliding
            });
        }
        public async override Task<ManagedToken> GetManagedTokenAsync(string key)
        {
            var tokens = await GetManagedTokensAsync();
            if (tokens == null)
            {
                return null;
            }
            ManagedToken token;
            if (tokens.TryGetValue(key, out token))
            {
                return token;
            }
            return null;
        }
        public async override Task RemoveManagedTokenAsync(string key)
        {
            var tokens = await GetManagedTokensAsync();
            if (tokens == null)
            {
                return;
            }
            if (tokens.ContainsKey(key))
            {
                tokens.Remove(key);
                await UpsertManagedTokensAsync(tokens);
            }
        }

        public async override Task UpsertManagedTokenAsync(string key, ManagedToken managedToken)
        {
            var tokens = await GetManagedTokensAsync();
            if (tokens == null)
            {
                tokens = new Dictionary<string, ManagedToken>();
            }
            tokens[key] = managedToken;
            await UpsertManagedTokensAsync(tokens);
        }

        public async override Task RemoveManagedTokensAsync()
        {
            var cacheKey = await GetCacheKeyAsync();
            await _cache.RemoveAsync(cacheKey);
        }
    }
}
