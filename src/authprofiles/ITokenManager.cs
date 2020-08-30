using System.Threading.Tasks;

namespace authprofiles
{
    public interface ITokenManager
    {
        Task<string> FetchAccessTokenAsync(string key, bool refresh = false);
    }
}
