using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Fhi.HelseId.Api.Authorization;
using Fhi.HelseId.Api.Handlers;
using Fhi.HelseId.Api.Services;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Api.ExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Use this for setting up ingoing api authentication (with an access token)
        /// </summary>
        public static void AddHelseIdApiAuthentication(this IServiceCollection services,
            IHelseIdApiKonfigurasjon config)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton(config);

            if (config.AuthUse)
            {
                services.AddSingleton<IAuthorizationHandler, ApiScopeHandler>();
                services.AddScoped<ICurrentUser, CurrentHttpUser>();
                services.AddScoped<IAccessTokenProvider, HttpContextAccessTokenProvider>();

                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddHelseIdJwtBearer(config);

                if (config.RequireContextIdentity)
                    services.AddHelseIdAuthorization(config);
                else
                    services.AddHelseIdApiAccessOnlyAuthorization(config);
            }
        }

        [Obsolete("Use AddHelseIdApiAuthentication() instead")]
        public static void ConfigureHelseIdApiAuthentication(this IServiceCollection services,
            IHelseIdApiKonfigurasjon config, IConfigurationSection configAuthSection)
        {
            services.AddHelseIdApiAuthentication(config);
        }

        public static bool SetupHelseIdAuthorizationControllers(this IServiceCollection services,
            IAutentiseringkonfigurasjon config)
        {
            if (!config.AuthUse)
            {
                return false;
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddControllers(cfg => { cfg.Filters.Add(new AuthorizeFilter(Policies.HidOrApi)); })
                .AddJsonOptions(
                    options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
            return true;
        }

        /// <summary>
        /// Use this for either User or Client credentials
        /// </summary>
        public static IServiceCollection ConfigureAuthenticationServices(this IServiceCollection services, IEnumerable<HelseIdApiOutgoingKonfigurasjon> apis)
        {
            foreach (var api in apis)
            {
                if (api.AuthUse)
                    ConfigureApiServices(services, api);
                else
                    ConfigureApiServicesNoAuth(services, api);
            }

            return services;
        }
        /// <summary>
        /// Use this for Apis that need to send access tokens onwards
        /// </summary>
        public static IServiceCollection ConfigureAuthenticationServicesForApis(this IServiceCollection services, IEnumerable<HelseIdApiOutgoingKonfigurasjon> apis)
        {
            foreach (var api in apis)
            {
                if (api.AuthUse)
                    ConfigureApiServicesInApis(services, api);
                else
                    ConfigureApiServicesNoAuth(services, api);
            }
            return services;
        }

        private static IHttpClientBuilder ConfigureApiServices(this IServiceCollection services, HelseIdApiOutgoingKonfigurasjon api)
        {
            return services.AddUserAccessTokenClient(api.Name, client =>
                {
                    client.BaseAddress = api.Uri;
                    client.Timeout = TimeSpan.FromMinutes(10);
                })
                .AddHttpMessageHandler<AuthHeaderHandler>();
        }

        private static IHttpClientBuilder ConfigureApiServicesNoAuth(this IServiceCollection services, HelseIdApiOutgoingKonfigurasjon api)
        {
            return services.AddHttpClient(api.Name, client =>
            {
                client.BaseAddress = api.Uri;
                client.Timeout = TimeSpan.FromMinutes(10);
            });
        }

        private static IHttpClientBuilder ConfigureApiServicesInApis(this IServiceCollection services, HelseIdApiOutgoingKonfigurasjon api)
        {
            services.AddScoped<AuthHeaderHandlerForApi>();
            return services.AddHttpClient(api.Name, client =>
            {
                client.BaseAddress = api.Uri;
                client.Timeout = TimeSpan.FromMinutes(10);
            }).AddHttpMessageHandler<AuthHeaderHandlerForApi>();
        }








    }
}
