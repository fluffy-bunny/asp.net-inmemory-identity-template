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
        //     Provides a central transformation point to change the specified principal. Note:
        //     this will be run on each AuthenticateAsync call, so its safer to return a new
        //     ClaimsPrincipal if your transformation is not idempotent.
        //
        // Parameters:
        //   principal:
        //     The System.Security.Claims.ClaimsPrincipal to transform.
        //
        // Returns:
        //     The transformed principal.
        Task<IEnumerable<Claim>> GetClaimsAsync(ClaimsPrincipal principal);
    }
}
