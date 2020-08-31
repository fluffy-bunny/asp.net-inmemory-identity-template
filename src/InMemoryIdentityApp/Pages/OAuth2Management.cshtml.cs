using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using oauth2.helpers;

namespace InMemoryIdentityApp.Pages
{
    public class OAuth2ManagementModel : PageModel
    {
        private IOAuth2CredentialManager _oAuth2CredentialManager;
        private ITokenManager<GlobalDistributedCacheTokenStorage> _globalTokenManager;

        public OAuth2ManagementModel(
            IOAuth2CredentialManager oAuth2CredentialManager,
            ITokenManager<GlobalDistributedCacheTokenStorage> globalTokenManager)
        {
            _oAuth2CredentialManager = oAuth2CredentialManager;
            _globalTokenManager = globalTokenManager;
        }

        public OAuth2Credentials Creds { get; private set; }
        public ManagedToken ManagedToken { get; private set; }

        public async Task OnGetAsync()
        {
            Creds = await _oAuth2CredentialManager.GetOAuth2CredentialsAsync("test");
            if(Creds == null)
            {
                await _oAuth2CredentialManager.AddCredentialsAsync("test", new OAuth2Credentials
                {
                    Authority = "https://demo.identityserver.io",
                    ClientId = "m2m",
                    ClientSecret = "secret"
                });
                // this call will do all the discovery work if it hasn't been done yet.
                Creds = await _oAuth2CredentialManager.GetOAuth2CredentialsAsync("test");

            }
            ManagedToken = await _globalTokenManager.GetManagedTokenAsync("test",true);
        }
    }
}
