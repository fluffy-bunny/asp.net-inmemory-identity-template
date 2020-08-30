using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace authprofiles.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddTokenManagerServices(this IServiceCollection services)
        {
            services.AddTransient<ITokenManager, TokenManager>();
            return services;
        }
    }
}
