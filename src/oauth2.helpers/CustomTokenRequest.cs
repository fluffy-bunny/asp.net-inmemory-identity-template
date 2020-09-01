﻿using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public class CustomTokenRequest : ICustomTokenRequest
    {
        Dictionary<string, Func<ManagedToken, IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>>> _functions;
        public CustomTokenRequest(ILogger<CustomTokenRequest> logger)
        {
            _functions = new Dictionary<string, Func<ManagedToken, IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>>>
            {
                { "client_credentials", ExecuteClientCredentialsRequestAsync }
            };

        }
        static async Task<ManagedToken> ExecuteClientCredentialsRequestAsync(
           ManagedToken managedToken,
           IOAuth2CredentialManager oAuth2CredentialManager,
           CancellationToken cancellationToken = default)
        {
            var creds = await oAuth2CredentialManager.GetOAuth2CredentialsAsync(managedToken.CredentialsKey);
            var client = new HttpClient();
            var response = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = creds.DiscoveryDocumentResponse.TokenEndpoint,
                    ClientId = creds.ClientId,
                    ClientSecret = creds.ClientSecret,
                    Scope = managedToken.RequestedScope
                },
                cancellationToken);
            if (response.IsError)
            {
                throw new Exception(response.Error);
            }
            managedToken.RefreshToken = response.RefreshToken;
            managedToken.AccessToken = response.AccessToken;
            managedToken.ExpiresIn = response.ExpiresIn;
            return managedToken;
        }
        public void AddTokenRequestFunction(string key, Func<ManagedToken, IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>> func)
        {
            _functions.Add(key, func);
        }

        public Func<ManagedToken, IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>> GetTokenRequestFunc(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) 
            {
                return null;
            }
            Func<ManagedToken, IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>> func;
            _functions.TryGetValue(key, out func);
            return func;
        }
    }
}