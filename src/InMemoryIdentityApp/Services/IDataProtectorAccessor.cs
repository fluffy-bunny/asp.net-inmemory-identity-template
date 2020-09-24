using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryIdentityApp.Services
{
    public interface IDataProtectorAccessor
    {
        IDataProtector GetAppProtector();
    }
}

 