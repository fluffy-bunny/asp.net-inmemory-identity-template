using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private const string cacheKey = "serilog-enrichers-correlationId";
        private const string propertyName = "CorrelationId";

        private IHttpContextAccessor _httpContextAccessor;
        private ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor,
            ICorrelationContextAccessor correlationContextAccessor )
        {
            _httpContextAccessor = httpContextAccessor;
            _correlationContextAccessor = correlationContextAccessor;
        }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            HttpContext ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) return;

            var httpContextCache = ctx.Items[cacheKey];
            if (httpContextCache == null)
            {
                CorrelationContext correlationContext = _correlationContextAccessor.CorrelationContext;
                if (correlationContext == null) return;

                httpContextCache = correlationContext.CorrelationId;
                ctx.Items[cacheKey] = httpContextCache;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(propertyName, httpContextCache, true));

        }
    }
}
