using Microsoft.Extensions.DependencyInjection;
using oauth2.helpers.Default;
using oauth2.helpers.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace oauth2.helpers.Extensions
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddManagedTokenServices(this IServiceCollection services)
        {
            services.AddSingleton<ISerializer, Serializer>();
            services.AddSingleton<IDataProtectorAccessor, DataProtectorAccessor>();
            services.AddSingleton<IOpenIdConnectConfigurationAccessor, OpenIdConnectConfigurationAccessor>();
            services.AddSingleton<ICustomTokenRequestManager, CustomTokenRequestManager>();
            services.AddTransient<GlobalDistributedCacheTokenStorage>();
            services.AddTransient<SessionDistributedCacheTokenStorage>();
            services.AddTransient(typeof(ITokenManager<>), typeof(TokenManager<>));
            services.AddTransient<IOAuth2CredentialManager, OAuth2CredentialManager>();
            return services;
        }
    }
}
