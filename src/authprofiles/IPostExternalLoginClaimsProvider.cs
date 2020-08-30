using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace authprofiles
{
    //
    // Summary:
    //     Used by the ExternalLoginModel for claims
    //     transformation.
    public interface IPostExternalLoginClaimsProvider
    {
        //
        // Summary:
        //     Provides additional claims you can add to the external user that was just authenticated.
        //
        // Parameters:
        //   principal:
        //     The System.Security.Claims.ClaimsPrincipal to evaluate.
        //
        // Returns:
        //     The claims.
        Task<IEnumerable<Claim>> GetClaimsAsync(ClaimsPrincipal principal);
    }
}
