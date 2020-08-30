using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace jsonplaceholder.service.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddJsonPlaceholderServices(this IServiceCollection services)
        {
            services.AddTransient<IJsonPlaceholderService, JsonPlaceholderService>();
            return services;
        }
    }
}
