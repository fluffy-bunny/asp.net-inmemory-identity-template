using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public interface IOpenIdConnectConfigurationAccessor
    {
        Task<OpenIdConnectConfiguration> GetOpenIdConnectConfigurationAsync(string issuer);
    }
}
