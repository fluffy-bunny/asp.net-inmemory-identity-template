using System.Threading;
using System.Threading.Tasks;

namespace oauth2.helpers
{
    public interface ITokenManager<T> where T: TokenStorage  
    {
        Task<ManagedToken> AddManagedTokenAsync(string key, ManagedToken tokenConfig);
        Task RemoveManagedTokenAsync(string key);
        Task<ManagedToken> GetManagedTokenAsync(string key, bool forceRefresh = false, CancellationToken cancellationToken = default);
        Task RemoveAllManagedTokenAsync();
    }
}
