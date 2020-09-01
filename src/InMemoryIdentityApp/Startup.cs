using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using authprofiles;
using jsonplaceholder.service.Extensions;
using authprofiles.Extensions;
using oauth2.helpers.Extensions;
using oauth2.helpers;

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
                services.AddManagedTokenServices();
                services.AddJsonPlaceholderServices();

                // TODO: EVALUATE THIS
                // We want X-Frame-Options=deny for everything except a group of blessed domains
                services.AddAntiforgery(options =>
                {
                    options.SuppressXFrameOptionsHeader = true;
                    
                });
                services.AddCors(options => options.AddPolicy("CorsPolicy",
                    builder =>
                    {

                        builder.AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowAnyOrigin();
                    }));
                // set forward header keys to be the same value as request's header keys
                // so that redirect URIs and other security policies work correctly.
                var aspNETCORE_FORWARDEDHEADERS_ENABLED = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED"), "true", StringComparison.OrdinalIgnoreCase);
                if (aspNETCORE_FORWARDEDHEADERS_ENABLED)
                {
                    //To forward the scheme from the proxy in non-IIS scenarios
                    services.Configure<ForwardedHeadersOptions>(options =>
                    {
                        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                        // Only loopback proxies are allowed by default.
                        // Clear that restriction because forwarders are enabled by explicit 
                        // configuration.
                        options.KnownNetworks.Clear();
                        options.KnownProxies.Clear();
                    });
                }

                services.AddDefaultCorrelationId();
                services.AddEnrichers();
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

                services.AddInMemoryIdentity<ApplicationUser, ApplicationRole>().AddDefaultTokenProviders();
                services.ConfigureExternalCookie(config =>
                {
                    config.Cookie.SameSite = SameSiteMode.None;
                });
                services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
          

                    options.Cookie.Name = $"{Configuration["applicationName"]}.AspNetCore.Identity.Application";
                    options.LoginPath = $"/Identity/Account/Login"; 
                    options.LogoutPath = $"/Identity/Account/Logout";
                    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                    options.Events = new CookieAuthenticationEvents()
                    {
                        
                        OnRedirectToLogin = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == StatusCodes.Status200OK)
                            {
                                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == StatusCodes.Status200OK)
                            {
                                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                });
                services.AddAuthentication<ApplicationUser>(Configuration);
                services.AddSingleton<IPostExternalLoginClaimsProvider, PostExternalLoginClaimsProvider>();

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
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            }
            catch (Exception ex)
            {
                _deferedException = ex;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env,
            IServiceProvider serviceProvider,
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
            var oAuth2CredentialManager = serviceProvider.GetRequiredService<IOAuth2CredentialManager>();


            oAuth2CredentialManager.AddCredentialsAsync("test", new OAuth2Credentials
            {
                Authority = "https://demo.identityserver.io",
                ClientId = "m2m",
                ClientSecret = "secret"
            }).GetAwaiter().GetResult();

            var globalTokenManager = serviceProvider.GetRequiredService<ITokenManager<GlobalDistributedCacheTokenStorage>>();
            globalTokenManager.AddManagedTokenAsync("test", new oauth2.helpers.ManagedToken
            {
                CredentialsKey = "test",
                RequestFunctionKey = "client_credentials",
                RequestedScope = null // everything
            }).GetAwaiter().GetResult(); 

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseForwardedHeaders();
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
