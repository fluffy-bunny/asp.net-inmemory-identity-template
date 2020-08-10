 
using InMemoryIdentityApp.Constants;
using InMemoryIdentityApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryIdentityApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class AuthStatusController : ControllerBase
    {

        private readonly ILogger<AuthStatusController> _logger;

        public AuthStatusController(ILogger<AuthStatusController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("display-name")]
        public async Task<string> GetDisplayNameAsync()
        {
            var displayName = (from item in User.Claims
                               where item.Type == "display-name"
                               select item.Value).FirstOrDefault();
            return displayName;
        }
        [HttpGet]
        [Route("claims")]
        public async Task<IEnumerable<ClaimHandle>> GetClaimsAsync()
        {
            var claims = from claim in User.Claims
                         let c = new ClaimHandle
                         {
                             Type = claim.Type,
                             Value = claim.Value
                         }
                         select c;
           
            return claims;
        }
        [HttpGet]
        [Route("oidc-session-details")]
        public async Task<OpenIdConnectSessionDetails> GetOpenIdConnectSessionDetailsAsync()
        {
            OpenIdConnectSessionDetails result = HttpContext.Session.Get<OpenIdConnectSessionDetails>(Wellknown.OIDCSessionKey);
            return result;
        }
    }
}
