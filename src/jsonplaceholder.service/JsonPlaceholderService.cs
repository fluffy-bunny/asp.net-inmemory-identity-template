using authprofiles;
using Common;
using jsonplaceholder.service.Models;
using Microsoft.Extensions.Logging;
using oauth2.helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace jsonplaceholder.service
{

    public class JsonPlaceholderService : IJsonPlaceholderService
    {
        private ITokenManager<SessionDistributedCacheTokenStorage> _tokenManager;
        private ILogger<JsonPlaceholderService> _logger;
 
        public JsonPlaceholderService(
            ITokenManager<SessionDistributedCacheTokenStorage> tokenManager,
            ILogger<JsonPlaceholderService> logger)
        {
            _tokenManager = tokenManager;
            _logger = logger;
        }
        async Task<HttpClient> CreateHttpClientAsync()
        {
            string baseUrl = "https://jsonplaceholder.typicode.com/";
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            var managedToken = await _tokenManager.GetManagedTokenAsync("test", true);
            if(managedToken == null)
            {
                await _tokenManager.AddManagedTokenAsync("test", new oauth2.helpers.ManagedToken
                {
                    CredentialsKey = "test",
                    RequestFunctionKey = "client_credentials",
                    RequestedScope = null // everything
                });
                managedToken = await _tokenManager.GetManagedTokenAsync("test",true);
            }
          
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                managedToken.AccessToken);
            
            return client;
        }
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            _logger.LogDebug("calling getUsersAsync.");
            var client = await CreateHttpClientAsync();
            HttpResponseMessage response = await client.GetAsync("/users");
            if (response.IsSuccessStatusCode)
            {
                string stringData = await response.Content.ReadAsStringAsync();

                List<User> users = JsonSerializer.Deserialize<List<User>>(stringData);

                return users;
            }
            _logger.LogError($"GetUserAsync:{ response.StatusCode}");
            throw new StatusCodeException(response.StatusCode);
        }
    }


}
