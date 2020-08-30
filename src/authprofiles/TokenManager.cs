using System;
using System.Threading.Tasks;

namespace authprofiles
{
    public class TokenManager : ITokenManager
    {
        public async Task<string> FetchAccessTokenAsync(string key, bool refresh = false)
        {
            // all fake.
            // IdentityModel has a good OAuth2 token fetching library that you can use to manage token lifetimes
            return Guid.NewGuid().ToString();
        }
    }
}
