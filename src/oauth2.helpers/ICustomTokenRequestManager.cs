using oauth2.helpers.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public interface ICustomTokenRequestManager
    {
        void AddTokenRequestFunction(string key, Func<ManagedToken, IServiceProvider,IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>> func);
        Func<ManagedToken, IServiceProvider,IOAuth2CredentialManager, CancellationToken, Task<ManagedToken>> GetTokenRequestFunc(string key);
    }
}
