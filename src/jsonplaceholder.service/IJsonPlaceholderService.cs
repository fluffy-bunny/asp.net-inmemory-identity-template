using jsonplaceholder.service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace jsonplaceholder.service
{
    public interface IJsonPlaceholderService
    {
        Task<IEnumerable<User>> GetUsersAsync();
    }


}
