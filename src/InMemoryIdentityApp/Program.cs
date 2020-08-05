using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Extensions;

namespace InMemoryIdentityApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, provider, loggerConfiguration) => {
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    loggerConfiguration
                    .ReadFrom.Configuration(configuration)
                    .Enrich.WithCorrelationIdEnricher(provider);
                 //   .Enrich.WithAspnetcoreHttpcontext(provider);

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
