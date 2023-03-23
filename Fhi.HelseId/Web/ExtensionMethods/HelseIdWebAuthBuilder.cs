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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Web.ExtensionMethods;

public class HelseIdWebAuthBuilder
{
    private readonly IServiceCollection services;
    private IConfiguration Configuration { get; }
    public IHelseIdWebKonfigurasjon HelseIdWebKonfigurasjon { get; }
    private readonly IConfigurationSection? helseIdKonfigurasjonSeksjon;
    private readonly IConfigurationSection? redirectSection;
    private readonly RedirectPagesKonfigurasjon redirectPagesKonfigurasjon;

    public HelseIdWebAuthBuilder(IConfiguration configuration, IServiceCollection services)
    {
        this.services = services;
        Configuration = configuration;
        helseIdKonfigurasjonSeksjon = Configuration.GetSection(nameof(HelseIdWebKonfigurasjon));
        if (helseIdKonfigurasjonSeksjon == null)
            throw new ConfigurationException($"Missing required configuration section {nameof(HelseIdWebKonfigurasjon)}");
        HelseIdWebKonfigurasjon = helseIdKonfigurasjonSeksjon.Get<HelseIdWebKonfigurasjon>();
        redirectSection = Configuration.GetSection(nameof(RedirectPagesKonfigurasjon));
        redirectPagesKonfigurasjon = redirectSection?.Get<RedirectPagesKonfigurasjon>()??new RedirectPagesKonfigurasjon();
        
    }

    /// <summary>
    /// Add this to the services section
    /// </summary>
    /// <param name="secretHandler">Default is ClientSecrets, add another if different, see wiki</param>
    /// <param name="configureMvc"></param>
    /// <param name="configureAuthentication"></param>
    /// <returns></returns>
    public IMvcBuilder AddHelseIdWebAuthentication(
        IHelseIdSecretHandler? secretHandler = null,
        Action<MvcOptions>? configureMvc = null,
        ConfigureAuthentication? configureAuthentication = null)
    {
        if (HelseIdWebKonfigurasjon.AuthUse)
        {
            if (redirectSection != null)
                services.Configure<RedirectPagesKonfigurasjon>(redirectSection);
            services.AddScoped<IGodkjenteHprKategoriListe, NoHprApprovals>();
            if (HelseIdWebKonfigurasjon.UseHprPolicy)
                services.AddSingleton<IAuthorizationHandler, HprGodkjenningAuthorizationHandler>();
            else if (HelseIdWebKonfigurasjon.UseHprNumber)
                services.AddSingleton<IAuthorizationHandler, HprAuthorizationHandler>();
                              
            services.AddSingleton<IWhitelist>(HelseIdWebKonfigurasjon.Whitelist);
            services.AddSingleton<IAutentiseringkonfigurasjon>(HelseIdWebKonfigurasjon);
            services.AddSingleton(HelseIdWebKonfigurasjon);
            services.Configure<HelseIdWebKonfigurasjon>(helseIdKonfigurasjonSeksjon);
            services.AddMemoryCache();
            services.AddScoped<IHprFactory, HprFactory>();
            services.AddSingleton<IAuthorizationHandler, SecurityLevelClaimHandler>();
            services.AddScoped<ICurrentUser, CurrentHttpUser>();
            
            

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        (var authorizeFilter, string policyName) = AddAuthentication( secretHandler, configureAuthentication);
        
        var mvcBuilder = services.AddControllers(config =>
        {
            if (HelseIdWebKonfigurasjon.AuthUse)
            {
                config.Filters.Add(authorizeFilter);
            }

            configureMvc?.Invoke(config);
        });

        if (HelseIdWebKonfigurasjon.AuthUse)
        {
            if (HelseIdWebKonfigurasjon.UseHprNumber)
                services.AddScoped<IAuthorizationHandler, HprAuthorizationHandler>();
            if (HelseIdWebKonfigurasjon.UseHprPolicy)
                services.AddScoped<IAuthorizationHandler, HprGodkjenningAuthorizationHandler>();
        }

        return mvcBuilder;
    }

    private  AuthenticationBuilder AddHelseIdAuthentication(IServiceCollection services)
    {
        var builder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = HelseIdContext.Scheme;
        });
        return builder;
    }


    protected virtual (AuthorizeFilter AuthorizeFilter, string PolicyName) AddAuthentication(
        IHelseIdSecretHandler? secretHandler = null,
        ConfigureAuthentication? configureAuthentication = null)
    {
        const double tokenRefreshBeforeExpirationTime = 2;
        AddHelseIdAuthentication(services)
            .AddCookie(options =>
            {
                options.DefaultHelseIdOptions(HelseIdWebKonfigurasjon, redirectPagesKonfigurasjon);

                configureAuthentication?.ConfigureCookieAuthentication?.Invoke(options);
            })
            .AddOpenIdConnect(HelseIdContext.Scheme, options =>
            {
                options.DefaultHelseIdOptions(HelseIdWebKonfigurasjon, redirectPagesKonfigurasjon, secretHandler);

                configureAuthentication?.ConfigureOpenIdConnect?.Invoke(options);
            })
            .AddAutomaticTokenManagement(options => options.DefaultHelseIdOptions(tokenRefreshBeforeExpirationTime));   // For å kunne ha en lengre sesjon,  håndterer refresh token

        (var authPolicy, string policyName) = AddHelseIdAuthorizationPolicy(services);

        return (new AuthorizeFilter(authPolicy), policyName);
    }


    public void UseHelseIdProtectedPaths(IApplicationBuilder app,
        IReadOnlyCollection<PathString> excludeList, bool overrideDefaults = false)
    {
        if (!HelseIdWebKonfigurasjon.AuthUse)
            return;
        var excluded = overrideDefaults ? new List<PathString>() : new List<PathString>
        {
            "/favicon.ico",
            redirectPagesKonfigurasjon.Forbidden,
            redirectPagesKonfigurasjon.LoggedOut,
            redirectPagesKonfigurasjon.Statuscode
        };
        if (excludeList.Any())
            excluded.AddRange(excludeList);

        app.UseProtectPaths(new ProtectPathsOptions(DeterminePresidingPolicy(), redirectPagesKonfigurasjon.Forbidden)
        {
            Exclusions = excluded
        });
    }
    

    public  (AuthorizationPolicy AuthPolicy, string PolicyName) AddHelseIdAuthorizationPolicy(IServiceCollection services)
    {
        var authenticatedPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        var hidOrApiPolicy = new AuthorizationPolicyBuilder()
            .Combine(authenticatedPolicy)
            .AddRequirements(new SecurityLevelOrApiRequirement())
            .Build();

        if (HelseIdWebKonfigurasjon.UseHprNumber)
        {
            var hprNumberPolicyBuilder = new AuthorizationPolicyBuilder()
                .Combine(hidOrApiPolicy);
            hprNumberPolicyBuilder.Requirements.Add(new HprAuthorizationRequirement());
            var hprNumberPolicy = hprNumberPolicyBuilder.Build();

            if (HelseIdWebKonfigurasjon.UseHprPolicy)
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


    /// <summary>
    /// Determine the presiding policy from configuration.
    /// Will return Policies.Authenticated if no other policies are configured.
    /// </summary>
    /// <returns></returns>
    private string DeterminePresidingPolicy()
        => new[]
            {
                new { PolicyActive = HelseIdWebKonfigurasjon.UseHprNumber && HelseIdWebKonfigurasjon.UseHprPolicy, Policy = Policies.GodkjentHprKategoriPolicy},
                new { PolicyActive = HelseIdWebKonfigurasjon.UseHprNumber, Policy = Policies.HprNummer },
                new { PolicyActive = true, Policy = Policies.HidOrApi },
                new { PolicyActive = true, Policy = Policies./*Hid*/Authenticated }
            }
            .ToList()
            .First(p => p.PolicyActive).Policy;
}

public class NoHprApprovals : GodkjenteHprKategoriListe
{
    public NoHprApprovals()
    {
    }
}