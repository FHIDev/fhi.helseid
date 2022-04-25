using System;
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
    }
}
