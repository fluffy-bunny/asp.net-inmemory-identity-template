using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InMemoryIdentityApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace InMemoryIdentityApp.Pages
{
    public class OIDCIFrameResultModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<OIDCIFrameResultModel> _logger;
        public string Error { get; set; }
        public Dictionary<string, string> OIDC { get; set; }
        public string FrameLoginProxy { get; private set; }

        public OIDCIFrameResultModel(SignInManager<IdentityUser> signInManager, ILogger<OIDCIFrameResultModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        public async Task OnGet(string error = null)
        {
            FrameLoginProxy = $"{Request.Scheme}://{Request.Host}/iFrameLoginProxyModel";
            Error = error;
            if (string.IsNullOrEmpty(Error))
            {
                OIDC = await HarvestOidcDataAsync();
            }
        }
        private async Task<Dictionary<string, string>> HarvestOidcDataAsync()
        {
            var at = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "access_token");
            var idt = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "id_token");
            var rt = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "refresh_token");
            var tt = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "token_type");
            var ea = await HttpContext.GetTokenAsync(IdentityConstants.ExternalScheme, "expires_at");

            var oidc = new Dictionary<string, string>
            {
                {"access_token", at},
                {"id_token", idt},
                {"refresh_token", rt},
                {"token_type", tt},
                {"expires_at", ea}
            };
            return oidc;
        }
    }
}
