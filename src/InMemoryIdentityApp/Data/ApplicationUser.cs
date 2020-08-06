using Microsoft.AspNetCore.Identity;

namespace InMemoryIdentityApp.Data
{
    public class ApplicationUser : MemoryUser
    {
        public string DisplayName { get; set; }
    }
}
