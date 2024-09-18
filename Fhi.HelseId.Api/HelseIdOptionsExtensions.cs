using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using HelseId.Samples.Common.ApiDPoPValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using Fhi.HelseId.Api.ApiDPoPValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

                    options.Events ??= new JwtBearerEvents();
                    options.Events.OnMessageReceived = context =>
                    {
                        // Per HelseID's security profile, an API endpoint can accept *either*
                        // a DPoP access token *or* a Bearer access token, but not both.

                        // This ensures that the received access token is a DPoP token:
                        if (context.Request.GetDPoPAccessToken(out var dPopToken))
                        {
                            context.Token = dPopToken;
                        }
                        else
                        {
                            // Do not accept a bearer token:
                            context.Fail("Expected a valid DPoP token");
                        }
                        return Task.CompletedTask;
                    };

                    options.Events.OnTokenValidated = async tokenValidatedContext =>
                    {
                        try
                        {
                            // This functionality validates the DPoP proof
                            // https://www.ietf.org/archive/id/draft-ietf-oauth-dpop-16.html#name-checking-dpop-proofs

                            // Get the DPoP proof:
                            var request = tokenValidatedContext.HttpContext.Request;
                            if (!request.GetDPoPProof(out var dPopProof))
                            {
                                tokenValidatedContext.Fail("Missing DPoP proof");
                                return;
                            }

                            // Get the access token:
                            request.GetDPoPAccessToken(out var accessToken);

                            // Get the cnf claim from the access token:
                            var cnfClaimValue = tokenValidatedContext.Principal!.FindFirstValue(JwtClaimTypes.Confirmation);

                            var data = new DPoPProofValidationData(request, dPopProof!, accessToken!, cnfClaimValue);

                            var dPopProofValidator = tokenValidatedContext.HttpContext.RequestServices.GetRequiredService<DPoPProofValidator>();
                            var validationResult = await dPopProofValidator.Validate(data);
                            if (validationResult.IsError)
                            {
                                tokenValidatedContext.Fail(validationResult.ErrorDescription!);
                            }
                        }
                        catch (Exception)
                        {
                            tokenValidatedContext.Fail("Invalid token!");
                        }
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
