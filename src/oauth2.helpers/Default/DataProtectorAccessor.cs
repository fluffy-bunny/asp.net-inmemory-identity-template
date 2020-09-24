using Microsoft.AspNetCore.DataProtection;
using oauth2.helpers.Services;

namespace oauth2.helpers.Default
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
            return GetProtector("5c448492-3b24-4f89-a694-751ca5a1b300");
        }

        public IDataProtector GetProtector(string name)
        {
            var protector = _provider.CreateProtector(name);
            return protector;
        }
    }
}

