using System.Threading.Tasks;

namespace oauth2.helpers
{
    public interface IOAuth2CredentialManager
    {
        Task<OAuth2Credentials> GetOAuth2CredentialsAsync(string key);
        Task AddCredentialsAsync(string key, OAuth2Credentials creds);
    }
}
