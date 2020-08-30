using SharedModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace authprofiles
{
    public class PostExternalLoginClaimsProvider : IPostExternalLoginClaimsProvider
    {
        public async Task<IEnumerable<Claim>> GetClaimsAsync(ClaimsPrincipal principal)
        {
 
            /*
             * A user was just authenicated against some Identity Provider (IDP) like google
             * Google obviously doesn't know about your little app here.  You probably have another database that maps
             * the user to some services that this app provides.
             * 
             *  
             */

            // shows how to get a list of all the claims
            var claims = from claim in principal.Claims
                         let c = new ClaimHandle
                         {
                             Type = claim.Type,
                             Value = claim.Value
                         }
                         select c;

            // after you find the claim you want.  i.e. subject 
            // Lookup that subject in your database
            // get the claims that your database says to add 

            // lets add some some
            var additionalClaims = new List<Claim>            {
                new Claim("Transformed", DateTime.Now.ToString()),
                new Claim("Role", "Bunny-Tamer")
            };
           
            return additionalClaims;
        }
    }
}
