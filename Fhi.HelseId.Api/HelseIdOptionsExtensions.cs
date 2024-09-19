using Fhi.HelseId.Api.ApiDPoPValidation;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Fhi.HelseId.Api
{
    public static class HelseIdOptionsExtensions
    {
        public static AuthenticationBuilder AddHelseIdJwtBearer(this AuthenticationBuilder authenticationBuilder,
            IHelseIdApiKonfigurasjon configAuth)
        {
            var builder = authenticationBuilder.AddJwtBearer(
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

                    if (configAuth.AllowDPoPTokens || configAuth.RequireDPoPTokens)
                    {
                        ConfigureDPoPTokenHandling(options, configAuth);
                    }
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

        private static void ConfigureDPoPTokenHandling(JwtBearerOptions options, IHelseIdApiKonfigurasjon configAuth)
        {
            options.Events ??= new JwtBearerEvents();
            options.Events.OnMessageReceived = context =>
            {
                var tokenHandler = context.HttpContext.RequestServices.GetRequiredService<IDPoPTokenHandler>();
                tokenHandler.ValidateAuthorizationHeader(context);

                return Task.CompletedTask;
            };

            options.Events.OnTokenValidated = async tokenValidatedContext =>
            {
                var tokenHandler = tokenValidatedContext.HttpContext.RequestServices.GetRequiredService<IDPoPTokenHandler>();
                await tokenHandler.ValidateDPoPProof(tokenValidatedContext);
            };
        }
    }
}
