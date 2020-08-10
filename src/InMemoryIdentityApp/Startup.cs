using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using InMemoryIdentityApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using InMemoryIdentityApp.Extensions;
using Microsoft.AspNetCore.Identity.Extensions;
using Microsoft.Extensions.Logging;
using Serilog.Enrichers.Extensions;
using CorrelationId.DependencyInjection;
using CorrelationId;

namespace InMemoryIdentityApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IHostEnvironment _hostingEnvironment; 
        private ILogger _logger;
        private Exception _deferedException;

        public Startup(IConfiguration configuration,
             IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _logger = new LoggerBuffered(LogLevel.Debug);
            _logger.LogInformation($"Create Startup {hostingEnvironment.ApplicationName} - {hostingEnvironment.EnvironmentName}");

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddDefaultCorrelationId();
                services.AddEnrichers();
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

                services.AddInMemoryIdentity<ApplicationUser, ApplicationRole>().AddDefaultTokenProviders();

                services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.Name = $"{Configuration["applicationName"]}.AspNetCore.Identity.Application";
                });
                services.AddAuthentication<ApplicationUser>(Configuration);

                services.AddControllers();
                services.AddRazorPages();

                // Adds a default in-memory implementation of IDistributedCache.
                services.AddDistributedMemoryCache();

                services.AddSession(options =>
                {
                    options.Cookie.IsEssential = true;
                    options.Cookie.Name = $"{Configuration["applicationName"]}.Session";
                // Set a short timeout for easy testing.
                    options.IdleTimeout = TimeSpan.FromSeconds(3600);
                    options.Cookie.HttpOnly = true;
                });
            }
            catch (Exception ex)
            {
                _deferedException = ex;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILogger<Startup> logger)
        {
            if (_logger is LoggerBuffered)
            {
                (_logger as LoggerBuffered).CopyToLogger(logger);
            }
            _logger = logger;
            _logger.LogInformation("Configure");
            if (_deferedException != null)
            {
                _logger.LogError(_deferedException.Message);
                throw _deferedException;
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCorrelationId();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
