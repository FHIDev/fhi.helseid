﻿using System;
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
        /// This extension enables multiple scopes defined in the configuration by one handler, and single scope by another
        /// </summary>
        public static void AddHelseIdApiAuthentication(this IServiceCollection services,
            IHelseIdApiKonfigurasjon config)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton(config);

            if (config.AuthUse)
            {
                services.AddScoped<ICurrentUser, CurrentHttpUser>();
                if (config.ApiScope.Contains(',')) // We know there are multiple scopes if a komma is present
                    services.AddSingleton<IAuthorizationHandler, ApiMultiScopeHandler>();
                else
                    services.AddSingleton<IAuthorizationHandler, ApiSingleScopeHandler>();
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

        

        /// <summary>
        /// Use this for either User or Client credentials
        /// </summary>
        public static bool AddHelseIdAuthorizationControllers(this IServiceCollection services,
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

        public static IServiceCollection AddHelseIdAuthenticationServices(this IServiceCollection services, IEnumerable<HelseIdApiOutgoingKonfigurasjon> apis)
        {
            foreach (var api in apis)
            {
                if (api.AuthUse)
                    AddHelseIdApiServices(services, api);
                else
                    AddHelseIdApiServicesNoAuth(services, api);
            }

            return services;
        }

        /// <summary>
        /// Use this for Apis that need to send access tokens onwards
        /// </summary>
        public static IServiceCollection AddHelseIdAuthenticationServicesForApis(this IServiceCollection services, IEnumerable<HelseIdApiOutgoingKonfigurasjon> apis)
        {
            services.AddScoped<AuthHeaderHandlerForApi>();
            foreach (var api in apis)
            {
                if (api.AuthUse)
                    AddHelseIdApiServicesForApi(services, api);
                else
                    AddHelseIdApiServicesNoAuth(services, api);
            }
            return services;
        }



        private static IHttpClientBuilder AddHelseIdApiServices(this IServiceCollection services, IApiOutgoingKonfigurasjon api)
        {
            return services.AddUserAccessTokenHttpClient(api.Name, configureClient: client =>
                {
                    client.BaseAddress = api.Uri;
                    client.Timeout = TimeSpan.FromMinutes(10);
                })
                .AddHttpMessageHandler<AuthHeaderHandler>();
        }

        private static IHttpClientBuilder AddHelseIdApiServicesNoAuth(this IServiceCollection services, IApiOutgoingKonfigurasjon api)
        {
            return services.AddHttpClient(api.Name, client =>
            {
                client.BaseAddress = api.Uri;
                client.Timeout = TimeSpan.FromMinutes(10);
            });
        }

        private static IHttpClientBuilder AddHelseIdApiServicesForApi(this IServiceCollection services, IApiOutgoingKonfigurasjon api)
        {
            return services.AddHttpClient(api.Name, client =>
            {
                client.BaseAddress = api.Uri;
                client.Timeout = TimeSpan.FromMinutes(10);
            }).AddHttpMessageHandler<AuthHeaderHandlerForApi>();
        }

        #region Obsolete Errors
        [Obsolete("Use AddHelseIdAuthenticationServices() instead", true)]
        public static IServiceCollection ConfigureAuthenticationServices(this IServiceCollection services, IEnumerable<HelseIdApiOutgoingKonfigurasjon> apis)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use AddHelseIdAuthenticationServicesForApis() instead", true)]
        public static IServiceCollection ConfigureAuthenticationServicesForApis(this IServiceCollection services, IEnumerable<HelseIdApiOutgoingKonfigurasjon> apis)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use AddHelseIdApiAuthentication() instead", true)]
        public static void ConfigureHelseIdApiAuthentication(this IServiceCollection services,
            IHelseIdApiKonfigurasjon config, IConfigurationSection configAuthSection)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
