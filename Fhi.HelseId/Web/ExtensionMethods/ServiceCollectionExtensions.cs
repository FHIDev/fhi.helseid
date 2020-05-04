using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Fhi.HelseId.Web.Middleware;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;


namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddHelseIdAuthentication(this IServiceCollection services)
        {
            var builder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = HelseIdContext.Scheme;
            });
            return builder;
        }

        public static (AuthorizationPolicy AuthPolicy, string PolicyName) AddHelseIdAuthorizationPolicy(this IServiceCollection services,
            IHelseIdHprFeatures helseIdFeatures,
            IHprFeatureFlags hprFeatures,
            IWhitelist whitelist)
        {
            var authenticatedHidUserPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(IdentityClaims.SecurityLevel, SecurityLevel.Level4)
                .Build();
            if (helseIdFeatures.UseHprNumber)
            {
                var hprNumberPolicyBuilder = new AuthorizationPolicyBuilder()
                    .Combine(authenticatedHidUserPolicy);
                hprNumberPolicyBuilder.Requirements.Add(new HprAuthorizationRequirement());
                var hprNumberPolicy= hprNumberPolicyBuilder.Build();

                if (hprFeatures.UseHpr && hprFeatures.UseHprPolicy)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .Combine(hprNumberPolicy);
                    policy.Requirements.Add(new LegeAuthorizationRequirement());
                    var legePolicy = policy.Build();
                    services.AddAuthorization(config =>
                    {
                        config.AddPolicy(Policies.HidAuthenticated, authenticatedHidUserPolicy);
                        config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                        config.AddPolicy(Policies.LegePolicy, legePolicy);
                        config.DefaultPolicy = legePolicy;
                    });
                    return (legePolicy, Policies.LegePolicy);
                }

                services.AddAuthorization(config =>
                {
                    config.AddPolicy(Policies.HidAuthenticated, authenticatedHidUserPolicy);
                    config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                    config.DefaultPolicy = hprNumberPolicy;
                });
                return (hprNumberPolicy, Policies.HprNummer);
            }

            services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.HidAuthenticated, authenticatedHidUserPolicy);
                config.DefaultPolicy = authenticatedHidUserPolicy;
            });
            return (authenticatedHidUserPolicy, Policies.HidAuthenticated);
        }

        public static void UseHelseIdProtectedPaths(this IApplicationBuilder app,
            IHelseIdWebKonfigurasjon config, IRedirectPagesKonfigurasjon redirect, IReadOnlyCollection<PathString> excludeList)
        {
            if (!config.AuthUse)
                return;
            var excluded = new List<PathString>
            {
                "/favicon.ico",
                redirect.Forbidden,
                redirect.LoggedOut,
                redirect.Statuscode
            };
            if (excludeList != null && excludeList.Any())
                excluded.AddRange(excludeList);
            app.UseProtectPaths(new ProtectPathsOptions(config.UseHprNumber ? Policies.HprNummer : Policies.HidAuthenticated, redirect.Forbidden)
            {
                Exclusions = excluded
            });
        }


        public static (bool Result, string PolicyName) AddHelseIdWebAuthentication(this IServiceCollection services,
            IHelseIdWebKonfigurasjon helseIdKonfigurasjon,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon,
            IHprFeatureFlags hprKonfigurasjon,
            IWhitelist whitelist)
        {
            if (!helseIdKonfigurasjon.AuthUse)
                return (false, "");
            services.AddScoped<IHprFactory, HprFactory>();
            services.AddScoped<ICurrentUser, CurrentHttpUser>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            (var authorizeFilter, string policyName) = AddAuthentication(services, helseIdKonfigurasjon, redirectPagesKonfigurasjon, hprKonfigurasjon, whitelist);
            services.AddControllers(config => config.Filters.Add(authorizeFilter));
            if (helseIdKonfigurasjon.UseHprNumber)
                services.AddScoped<IAuthorizationHandler, HprAuthorizationHandler>();
            if (hprKonfigurasjon.UseHpr && hprKonfigurasjon.UseHprPolicy)
                services.AddScoped<IAuthorizationHandler, LegeAuthorizationHandler>();
            return (true, policyName);
        }

        /// <summary>
        /// Normally used by AddHelseIdWebAuthentication
        /// </summary>
        public static (AuthorizeFilter AuthorizeFilter, string PolicyName) AddAuthentication(IServiceCollection services,
            IHelseIdWebKonfigurasjon helseIdKonfigurasjon,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon,
            IHprFeatureFlags hprKonfigurasjon,
            IWhitelist whitelist)
        {
            const double tokenRefreshBeforeExpirationTime = 2;

            services.AddHelseIdAuthentication()
                .AddCookie(options =>
                {
                    options.DefaultHelseIdOptions(helseIdKonfigurasjon, redirectPagesKonfigurasjon);
                })
                .AddOpenIdConnect(HelseIdContext.Scheme, options =>
                {
                    options.DefaultHelseIdOptions(helseIdKonfigurasjon, redirectPagesKonfigurasjon);
                })
                .AddAutomaticTokenManagement(options => options.DefaultHelseIdOptions(tokenRefreshBeforeExpirationTime));   // For å kunne ha en lengre sesjon,  håndterer refresh token

            (var authPolicy, string policyName) = services.AddHelseIdAuthorizationPolicy(helseIdKonfigurasjon, hprKonfigurasjon, whitelist);

            return (new AuthorizeFilter(authPolicy), policyName);
        }


    }
}
