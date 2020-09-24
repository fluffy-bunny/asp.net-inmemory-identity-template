using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace oauth2.helpers.Services
{
    public interface IDataProtectorAccessor
    {
        IDataProtector GetAppProtector();
        IDataProtector GetProtector(string name);
    }
}

