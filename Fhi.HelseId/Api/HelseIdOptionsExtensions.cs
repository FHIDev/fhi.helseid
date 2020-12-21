using Fhi.HelseId.Altinn.Authorization;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
                }
            );
            return builder;
        }

        public static void AddAltinnAuthorizationPolicy(this IServiceCollection services, string policyName, string altinnServiceCode, int altinnServiceEditionCode)
        {
            services.AddAuthorization(
                config =>
                {
                    var altinnAccessPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddRequirements(new AltinnServiceAuthorizationRequirement(altinnServiceCode, altinnServiceEditionCode))
                        .Build();
                    config.AddPolicy(policyName, altinnAccessPolicy);
                });
        }

        public static void AddHelseIdAuthorization(this IServiceCollection services, IHelseIdApiKonfigurasjon configAuth)
        {
            services.AddAuthorization(
                config =>
                {
                    var authenticatedHidUserPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    var apiAccessPolicy = new AuthorizationPolicyBuilder()
                        .Combine(authenticatedHidUserPolicy)
                        .RequireScope(configAuth.ApiScope)
                        .Build();

                    config.DefaultPolicy = apiAccessPolicy;

                    config.AddPolicy(Policies.HidAuthenticated, authenticatedHidUserPolicy);
                    config.AddPolicy(Policies.ApiAccess, apiAccessPolicy);
                }
            );
        }



    }
}
