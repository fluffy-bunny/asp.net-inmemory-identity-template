using authprofiles;
using Common;
using jsonplaceholder.service.Models;
using Microsoft.Extensions.Logging;
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
        private ILogger<JsonPlaceholderService> _logger;
        ITokenManager _tokenManager;
        public JsonPlaceholderService(
            ITokenManager tokenManager,
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
            var access_token = await _tokenManager.FetchAccessTokenAsync("jsonplaceholder");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                access_token);
            
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
