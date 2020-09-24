using Microsoft.AspNetCore.DataProtection;

namespace InMemoryIdentityApp.Services
{
    public class DataProtectorAccessor : IDataProtectorAccessor
    {
        private IDataProtectionProvider _provider;

        public DataProtectorAccessor(IDataProtectionProvider provider)
        {
            _provider = provider;
        }
        public IDataProtector GetAppProtector()
        {
            var protector = _provider.CreateProtector("5c448492-3b24-4f89-a694-751ca5a1b300");
            return protector;
        }
    }
}

 