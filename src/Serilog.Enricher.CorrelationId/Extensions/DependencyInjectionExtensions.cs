using Microsoft.Extensions.DependencyInjection;

namespace Serilog.Enrichers.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEnrichers(this IServiceCollection services)
        {
            services.AddTransient<AspnetcoreHttpContextEnricher>();
            services.AddTransient<CorrelationIdEnricher>();
            
            return services;
        }

    }
}
