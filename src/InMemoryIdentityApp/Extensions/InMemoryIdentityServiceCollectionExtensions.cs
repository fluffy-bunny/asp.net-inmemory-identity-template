using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InMemoryIdentityApp.Extensions;
using InMemoryIdentityApp.Models;
using InMemoryIdentityApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace InMemoryIdentityApp.Extensions
{


    public static class InMemoryIdentityServiceCollectionExtensions
    {
        public static IdentityBuilder AddAuthentication<TUser>(this IServiceCollection services, IConfiguration configuration)
            where TUser : class => services.AddAuthentication<TUser>(configuration, null);

        public static IdentityBuilder AddAuthentication<TUser>(this IServiceCollection services,
            IConfiguration configuration,
            Action<IdentityOptions> setupAction)
            where TUser : class
        {
            // Services used by identity
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            var section = configuration.GetSection("oidc");
            var oAuth2SchemeRecords = new List<OpenIdConnectSchemeRecord>();
            section.Bind(oAuth2SchemeRecords);
            services.AddSingleton(oAuth2SchemeRecords);
            foreach (var record in oAuth2SchemeRecords)
            {
                var scheme = record.Scheme;
                authenticationBuilder.AddOpenIdConnect(scheme, scheme, options =>
                {
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.Authority = record.Authority;
                    options.CallbackPath = record.CallbackPath;
                    options.RequireHttpsMetadata = false;
                    if (!string.IsNullOrEmpty(record.ResponseType))
                    {
                        options.ResponseType = record.ResponseType;
                    }
                    options.GetClaimsFromUserInfoEndpoint = record.GetClaimsFromUserInfoEndpoint;
                    options.ClientId = record.ClientId;
                    options.ClientSecret = record.ClientSecret;
                    options.SaveTokens = true;

                    options.Events.OnMessageReceived = context =>
                    {
                        var dataProtectorAccessor = context.HttpContext.RequestServices.GetRequiredService<IDataProtectorAccessor>();
                        var protector = dataProtectorAccessor.GetAppProtector();

                        var key = context.HttpContext.Request.GetJsonCookie<string>(".oidc.memoryCacheKey", protector);
                        var dc = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                        dc.Set(key, context.ProtocolMessage);
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        var query = from item in context.Request.Query
                                    where string.Compare(item.Key, "prompt", true) == 0
                                    select item.Value;
                        if (query.Any())
                        {
                            var prompt = query.FirstOrDefault();
                            context.ProtocolMessage.Prompt = prompt;
                        }
                        var dataProtectorAccessor = context.HttpContext.RequestServices.GetRequiredService<IDataProtectorAccessor>();
                        var protector = dataProtectorAccessor.GetAppProtector();
                        context.Response.SetJsonCookie(".oidc.memoryCacheKey", Guid.NewGuid().ToString(), 60, protector);
                        if (record.AdditionalProtocolScopes != null && record.AdditionalProtocolScopes.Any())
                        {
                            string additionalScopes = "";
                            foreach (var item in record.AdditionalProtocolScopes)
                            {
                                additionalScopes += $" {item}";
                            }
                            context.ProtocolMessage.Scope += additionalScopes;
                        }
                        if (context.HttpContext.User.Identity.IsAuthenticated)
                        {
                            // assuming a relogin trigger, so we will make the user relogin on the IDP
                            context.ProtocolMessage.Prompt = "login";
                        }
                        context.ProtocolMessage.SetParameter("idp_code", "DT");
                        context.ProtocolMessage.SetParameter("custom", "{\"someCustomJson\":\"hi\"}");
                        /*
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            context.ProtocolMessage.AcrValues = "v1=google";
                        }
                        */
                        return Task.CompletedTask;
                    };
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                });
            }

            // claims transformation is run after every Authenticate call
            return new IdentityBuilder(typeof(TUser), services);
        }
    }
}
