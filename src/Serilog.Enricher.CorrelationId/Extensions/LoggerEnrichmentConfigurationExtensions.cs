using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using System;

namespace Serilog.Enrichers.Extensions
{
    
    public static class LoggerEnrichmentConfigurationExtensions
    {
        public static LoggerConfiguration WithCorrelationIdEnricher(
              this LoggerEnrichmentConfiguration enrichmentConfiguration,
              IServiceProvider serviceProvider)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));

            var enricher = serviceProvider.GetRequiredService<CorrelationIdEnricher>();

            return enrichmentConfiguration.With(enricher);
        }

        /// <summary>
        /// Enrich log events with Aspnetcore httpContext properties.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <param name="serviceProvider"></param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithAspnetcoreHttpcontext(
            this LoggerEnrichmentConfiguration enrichmentConfiguration,
            IServiceProvider serviceProvider,
            Func<IHttpContextAccessor, object> customMethod = null)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));

            var enricher = serviceProvider.GetRequiredService<AspnetcoreHttpContextEnricher>();

            if (customMethod != null)
                enricher.SetCustomAction(customMethod);

            return enrichmentConfiguration.With(enricher);
        }
    }
}
