using Fhi.HelseId.Api.Authorization;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Fhi.HelseId.Api
{
    public static class HelseIdOptionsExtensions
    {
        public static AuthenticationBuilder AddJwtBearerAndDpop(this AuthenticationBuilder builder, Action<JwtBearerOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>());
            return builder.AddScheme<JwtBearerOptions, CustomAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, displayName: null, configureOptions);
        }

        public static AuthenticationBuilder AddHelseIdJwtBearerAndDpop(this AuthenticationBuilder authenticationBuilder,
            IHelseIdApiKonfigurasjon configAuth)
        {
            var builder = authenticationBuilder.AddJwtBearerAndDpop(
                options =>
                {
                    options.Authority = configAuth.Authority;
                    options.Audience = configAuth.ApiName;
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.RefreshOnIssuerKeyNotFound = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireSignedTokens = true,
                        RequireAudience = true,
                        RequireExpirationTime = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                    };
                }
            );
            return builder;
        }

        /// <summary>
        /// This adds policies for requiring caller to be an user with an authenticated Identity.  Note that this might also be something else than HelseId, and might be confusing.
        /// This policy can not be used with Worker processes.
        /// This policy also adds the SecurityLevelOrApiRequirement requirement
        /// </summary>
        public static void AddHelseIdAuthorization(this IServiceCollection services, IHelseIdApiKonfigurasjon configAuth)
        {
            services.AddAuthorization(
                config =>
                {
                    var authenticatedPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    var hidOrApiPolicy = new AuthorizationPolicyBuilder()
                        .Combine(authenticatedPolicy)
                        .AddRequirements(new SecurityLevelOrApiRequirement())
                        .Build();

                    config.DefaultPolicy = hidOrApiPolicy;

                    config.AddPolicy(Policies.Authenticated, authenticatedPolicy);
                    config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
                }
            );
        }

        /// <summary>
        /// This policy only adds the SecurityLevelOrApiRequirement requirement
        /// </summary>
        public static void AddHelseIdApiAccessOnlyAuthorization(this IServiceCollection services, IHelseIdApiKonfigurasjon configAuth)
        {
            services.AddAuthorization(
                config =>
                {
                    var hidOrApiPolicy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new SecurityLevelOrApiRequirement())
                        .Build();

                    config.DefaultPolicy = hidOrApiPolicy;

                    config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
                }
            );
        }

    }
}
