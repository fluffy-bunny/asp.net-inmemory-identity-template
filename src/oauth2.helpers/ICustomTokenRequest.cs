using oauth2.helpers.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public interface ICustomTokenRequest
    {
        void AddTokenRequestFunction(string key, Func<ManagedToken, IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>> func);
        Func<ManagedToken, IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>> GetTokenRequestFunc(string key);
    }
}
