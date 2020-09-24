using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using oauth2.helpers.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public abstract class DistributedCacheTokenStorage<T>: TokenStorage
        where T: TokenStorage
    {
        protected ISerializer _serializer;
        protected ISymmetricEncryptor _encryptor;
        private IDistributedCache _cache;
        protected ILogger<T> _logger;
        public DistributedCacheTokenStorage(
            ISerializer serializer,
            ISymmetricEncryptor encryptor,
            IDistributedCache cache, 
            ILogger<T> logger)
        {
            _serializer = serializer;
            _encryptor = encryptor;
            _cache = cache;
            _logger = logger;
        }
        public abstract Task<string> GetCacheKeyAsync();

        protected async Task<Dictionary<string, ManagedToken>> GetManagedTokensAsync()
        {
            var cacheKey = await GetCacheKeyAsync();
            var cacheKeyHash = GetHash(cacheKey);
            var encrypted = await _cache.GetObjectFromJson<string>(cacheKeyHash);
            Dictionary<string, ManagedToken> managedTokens = null;
            if (string.IsNullOrWhiteSpace(encrypted))
            {
                managedTokens = new Dictionary<string, ManagedToken>();
                await UpsertManagedTokensAsync(managedTokens);
                return managedTokens;
            }
            var json = _encryptor.DecryptString(cacheKey, encrypted);
            var tokens = _serializer.Deserialize<Dictionary<string, ManagedToken>>(json);
            return tokens;
        }
       
        protected async Task UpsertManagedTokensAsync(Dictionary<string, ManagedToken> managedTokens)
        {
            var cacheKey = await GetCacheKeyAsync();
            var cacheKeyHash = GetHash(cacheKey);
            var json = _serializer.Serialize(managedTokens);
            var encrypted = _encryptor.EncryptString(cacheKey, json);

            await _cache.SetObjectAsJson(cacheKeyHash, encrypted, new DistributedCacheEntryOptions
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
            var cacheKeyHash = GetHash(cacheKey);
            await _cache.RemoveAsync(cacheKeyHash);
        }
        private string GetHash(string original)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(original));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
