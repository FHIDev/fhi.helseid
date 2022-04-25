using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Fhi.HelseId.Api;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Handlers;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Fhi.HelseId.Web.Middleware;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon,
            IWhitelist whitelist)
        {
            var authenticatedPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            var hidOrApiPolicy = new AuthorizationPolicyBuilder()
                .Combine(authenticatedPolicy)
                .AddRequirements(new SecurityLevelOrApiRequirement())
                .Build();

            if (helseIdFeatures.UseHprNumber)
            {
                var hprNumberPolicyBuilder = new AuthorizationPolicyBuilder()
                    .Combine(hidOrApiPolicy);
                hprNumberPolicyBuilder.Requirements.Add(new HprAuthorizationRequirement());
                var hprNumberPolicy= hprNumberPolicyBuilder.Build();

                if (hprFeatures.UseHpr && hprFeatures.UseHprPolicy)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .Combine(hprNumberPolicy);
                    policy.Requirements.Add(new HprGodkjenningAuthorizationRequirement());
                    var hprGodkjenningPolicy = policy.Build();
                    services.AddAuthorization(config =>
                    {
                        config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
                        config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                        config.AddPolicy(Policies.GodkjentHprKategoriPolicy, hprGodkjenningPolicy);
                        config.DefaultPolicy = hprGodkjenningPolicy;
                    });
                    return (hprGodkjenningPolicy, Policies.GodkjentHprKategoriPolicy);
                }

                services.AddAuthorization(config =>
                {
                    config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
                    config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                    config.DefaultPolicy = hprNumberPolicy;
                });
                return (hprNumberPolicy, Policies.HprNummer);
            }

            services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.HidOrApi, hidOrApiPolicy);
                config.DefaultPolicy = hidOrApiPolicy;
            });
            return (hidOrApiPolicy, Policies.HidOrApi);
        }

        public static void UseHelseIdProtectedPaths(this IApplicationBuilder app,
            IHelseIdWebKonfigurasjon config, 
            IHprFeatureFlags hprFlags,
            IRedirectPagesKonfigurasjon redirect, 
            IReadOnlyCollection<PathString> excludeList)
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

            app.UseProtectPaths(new ProtectPathsOptions(DeterminePresidingPolicy(config, hprFlags), redirect.Forbidden)
            {
                Exclusions = excluded
            });
        }

        private static (string PolicyName, IMvcBuilder MvcBuilder) AddHelseIdWebAuthenticationInternal(
            this IServiceCollection services,
            IHelseIdWebKonfigurasjon helseIdKonfigurasjon,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon,
            IHprFeatureFlags hprKonfigurasjon,
            IWhitelist whitelist,
            IHelseIdSecretHandler? secretHandler,
            Action<MvcOptions>? configureMvc,
            ConfigureAuthentication? configureAuthentication = null)
        {
            services.AddSingleton(helseIdKonfigurasjon);
            if (helseIdKonfigurasjon.AuthUse)
            {
                services.AddSingleton<IAuthorizationHandler, SecurityLevelClaimHandler>();
                services.AddScoped<IHprFactory, HprFactory>();
                services.AddScoped<ICurrentUser, CurrentHttpUser>();

                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            }

            (var authorizeFilter, string policyName) = services.AddAuthentication(
                helseIdKonfigurasjon, redirectPagesKonfigurasjon, hprKonfigurasjon, whitelist, 
                secretHandler, configureAuthentication);
            var mvcBuilder = services.AddControllers(config =>
            {
                if (helseIdKonfigurasjon.AuthUse)
                {
                    config.Filters.Add(authorizeFilter);
                }

                configureMvc?.Invoke(config);
            });

            if (helseIdKonfigurasjon.AuthUse)
            {
                if (helseIdKonfigurasjon.UseHprNumber)
                    services.AddScoped<IAuthorizationHandler, HprAuthorizationHandler>();
                if (hprKonfigurasjon.UseHpr && hprKonfigurasjon.UseHprPolicy)
                    services.AddScoped<IAuthorizationHandler, HprGodkjenningAuthorizationHandler>();
            }

            return (policyName, mvcBuilder);        
        }

        public static IMvcBuilder AddHelseIdWebAuthentication(this IServiceCollection services,
            IHelseIdWebKonfigurasjon helseIdKonfigurasjon,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon,
            IHprFeatureFlags hprKonfigurasjon,
            IWhitelist whitelist,
            IHelseIdSecretHandler? secretHandler,
            Action<MvcOptions>? configureMvc,
            ConfigureAuthentication? configureAuthentication = null)
        {
            (string policyName, IMvcBuilder mvcBuilder) = services.AddHelseIdWebAuthenticationInternal(
                helseIdKonfigurasjon, redirectPagesKonfigurasjon, hprKonfigurasjon, whitelist, 
                secretHandler, configureMvc, configureAuthentication
            );
            
            return mvcBuilder;
        }

        [Obsolete("Use AddHelseIdWebAuthentication() overload that returns IMvcBuilder")]
        public static (bool Result, string PolicyName) AddHelseIdWebAuthentication(this IServiceCollection services,
            IHelseIdWebKonfigurasjon helseIdKonfigurasjon,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon,
            IHprFeatureFlags hprKonfigurasjon,
            IWhitelist whitelist,
            IHelseIdSecretHandler? secretHandler = null)
        {
            (string policyName, IMvcBuilder mvcBuilder) = services.AddHelseIdWebAuthenticationInternal(
                helseIdKonfigurasjon, redirectPagesKonfigurasjon, hprKonfigurasjon, whitelist, secretHandler, null, null
            );

            return (true, policyName);
        }

        /// <summary>
        /// Normally used by AddHelseIdWebAuthentication
        /// </summary>
        public static (AuthorizeFilter AuthorizeFilter, string PolicyName) AddAuthentication(this IServiceCollection services,
            IHelseIdWebKonfigurasjon helseIdKonfigurasjon,
            IRedirectPagesKonfigurasjon redirectPagesKonfigurasjon,
            IHprFeatureFlags hprKonfigurasjon,
            IWhitelist whitelist,
            IHelseIdSecretHandler? secretHandler = null,
            ConfigureAuthentication? configureAuthentication = null)
        {
            const double tokenRefreshBeforeExpirationTime = 2;

            services.AddHelseIdAuthentication()
                .AddCookie(options =>
                {
                    options.DefaultHelseIdOptions(helseIdKonfigurasjon, redirectPagesKonfigurasjon);

                    configureAuthentication?.ConfigureCookieAuthentication?.Invoke(options);
                })
                .AddOpenIdConnect(HelseIdContext.Scheme, options =>
                {
                    options.DefaultHelseIdOptions(helseIdKonfigurasjon, redirectPagesKonfigurasjon, secretHandler);

                    configureAuthentication?.ConfigureOpenIdConnect?.Invoke(options);
                })
                .AddAutomaticTokenManagement(options => options.DefaultHelseIdOptions(tokenRefreshBeforeExpirationTime));   // For å kunne ha en lengre sesjon,  håndterer refresh token

            (var authPolicy, string policyName) = services.AddHelseIdAuthorizationPolicy(helseIdKonfigurasjon, hprKonfigurasjon, helseIdKonfigurasjon, whitelist);

            return (new AuthorizeFilter(authPolicy), policyName);
        }


        /// <summary>
        /// Determine the presiding policy from configuration.
        /// Will return Policies.Authenticated if no other policies are configured.
        /// </summary>
        /// <param name="helseIdWebKonfigurasjon"></param>
        /// <param name="hprFeatureFlags"></param>
        /// <returns></returns>
        private static string DeterminePresidingPolicy(IHelseIdWebKonfigurasjon helseIdWebKonfigurasjon, IHprFeatureFlags hprFeatureFlags)
            => new []
            {
                new { PolicyActive = helseIdWebKonfigurasjon.UseHprNumber && hprFeatureFlags.UseHprPolicy, Policy = Policies.GodkjentHprKategoriPolicy},
                new { PolicyActive = helseIdWebKonfigurasjon.UseHprNumber, Policy = Policies.HprNummer },
                new { PolicyActive = true, Policy = Policies.HidOrApi },
                new { PolicyActive = true, Policy = Policies./*Hid*/Authenticated }
            }
            .ToList()
            .First(p => p.PolicyActive).Policy;
    }
}
