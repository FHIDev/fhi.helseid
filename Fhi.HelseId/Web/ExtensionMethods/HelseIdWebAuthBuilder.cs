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
using Fhi.HelseId.Api.Handlers;

namespace Fhi.HelseId.Web.ExtensionMethods;

public class HelseIdWebAuthBuilder
{
    private readonly IServiceCollection services;
    private IConfiguration Configuration { get; }
    public IHelseIdWebKonfigurasjon? HelseIdWebKonfigurasjon { get; }
    private readonly IConfigurationSection? helseIdKonfigurasjonSeksjon;
    public RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; }
    public IHelseIdSecretHandler SecretHandler { get; set; }

    public HelseIdWebAuthBuilder(IConfiguration configuration, IServiceCollection services)
    {
        this.services = services;
        Configuration = configuration;
        helseIdKonfigurasjonSeksjon = Configuration.GetSection(nameof(HelseIdWebKonfigurasjon));
        if (helseIdKonfigurasjonSeksjon == null)
            throw new MissingConfigurationException($"Missing required configuration section {nameof(HelseIdWebKonfigurasjon)}");
        HelseIdWebKonfigurasjon = helseIdKonfigurasjonSeksjon.Get<HelseIdWebKonfigurasjon>();
        if (HelseIdWebKonfigurasjon == null)
            throw new MissingConfigurationException($"Missing required configuration {nameof(HelseIdWebKonfigurasjon)}");
        RedirectPagesKonfigurasjon = HelseIdWebKonfigurasjon.RedirectPagesKonfigurasjon;
        SecretHandler = new HelseIdNoAuthorizationSecretHandler(); // Default
    }

    /// <summary>
    /// Add this to the services section
    /// </summary>
    /// <param name="configureMvc"></param>
    /// <param name="configureAuthentication"></param>
    /// <returns></returns>
    public IMvcBuilder? AddHelseIdWebAuthentication(
        Action<MvcOptions>? configureMvc = null,
        ConfigureAuthentication? configureAuthentication = null)
    {
        if (HelseIdWebKonfigurasjon.AuthUse)
        {
            
            services.AddSingleton<IGodkjenteHprKategoriListe, NoHprApprovals>();
            if (HelseIdWebKonfigurasjon.UseHprPolicy)
            {
                services.AddSingleton<IAuthorizationHandler, HprGodkjenningAuthorizationHandler>();
                services.AddSingleton<IAuthorizationHandler, HprAuthorizationHandler>();
            }
            else if (HelseIdWebKonfigurasjon.UseHprNumber)
                services.AddSingleton<IAuthorizationHandler, HprAuthorizationHandler>();

            services.AddSingleton<IWhitelist>(HelseIdWebKonfigurasjon.Whitelist);
            services.AddSingleton<IAutentiseringkonfigurasjon>(HelseIdWebKonfigurasjon);
            services.AddMemoryCache();
            services.AddSingleton<IHprFactory, HprFactory>();
            services.AddSingleton<IAuthorizationHandler, SecurityLevelClaimHandler>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }
        else
        {
            services.AddSingleton<IAuthorizationHandler, NoAuthorizationHandler>();
            services.AddAuthentication("NoAuthentication")
                .AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>("NoAuthentication", null);
        }
        services.AddScoped<ICurrentUser, CurrentHttpUser>();
        services.AddSingleton(HelseIdWebKonfigurasjon);
        services.Configure<HelseIdWebKonfigurasjon>(helseIdKonfigurasjonSeksjon);
        services.AddHttpContextAccessor();
        services.AddSingleton<IRefreshTokenStore, RefreshTokenStore>();
        services.AddSingleton(SecretHandler);
        

        (var authorizeFilter, string policyName) = AddAuthentication(configureAuthentication);
            
        AddControllers(configureMvc, authorizeFilter);
        

        return MvcBuilder;
    }

    /// <summary>
    /// This property is set by the AddControllers method
    /// </summary>
    public IMvcBuilder? MvcBuilder { get; private set; }

    public HelseIdWebAuthBuilder AddControllers(Action<MvcOptions>? configureMvc, AuthorizeFilter? authorizeFilter)
    {
        MvcBuilder = services.AddControllers(config =>
        {
            //Unsure about this
            if (HelseIdWebKonfigurasjon.AuthUse && authorizeFilter is not null)
            {
                config.Filters.Add(authorizeFilter!);
            }

            configureMvc?.Invoke(config);
        });
        return this;
    }

    /// <summary>
    /// Add Authentication to the services section
    /// </summary>
    private AuthenticationBuilder AddHelseIdAuthentication()
    {
        var builder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = HelseIdContext.Scheme;
        });
        return builder;
    }


    protected virtual (AuthorizeFilter AuthorizeFilter, string PolicyName) AddAuthentication(ConfigureAuthentication? configureAuthentication = null)
    {
        const double tokenRefreshBeforeExpirationTime = 2;
        if (HelseIdWebKonfigurasjon.AuthUse)
        {
            AddHelseIdAuthentication()
                .AddCookie(options =>
                {
                    options.DefaultHelseIdOptions(HelseIdWebKonfigurasjon, RedirectPagesKonfigurasjon);

                    configureAuthentication?.ConfigureCookieAuthentication?.Invoke(options);
                })
                .AddOpenIdConnect(HelseIdContext.Scheme, options =>
                {
                    options.DefaultHelseIdOptions(HelseIdWebKonfigurasjon, RedirectPagesKonfigurasjon, SecretHandler);

                    configureAuthentication?.ConfigureOpenIdConnect?.Invoke(options);
                })
                .AddAutomaticTokenManagement(options => options.DefaultHelseIdOptions(tokenRefreshBeforeExpirationTime));   // For å kunne ha en lengre sesjon,  håndterer refresh token
        }

        (var authPolicy, string policyName) = AddHelseIdAuthorizationPolicy();

        return (new AuthorizeFilter(authPolicy), policyName);
    }

    /// <summary>
    /// Add this to the app section, used to trigger the authentication login process for files and endpoints that are otherwise not protected. Enable this by setting UseProtectedPaths. 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="excludeList"></param>
    /// <param name="overrideDefaults"></param>
    public void UseHelseIdProtectedPaths(IApplicationBuilder app,
        IReadOnlyCollection<PathString> excludeList, bool overrideDefaults = false)
    {
        if (!HelseIdWebKonfigurasjon.AuthUse || !HelseIdWebKonfigurasjon.UseProtectedPaths) return;
        var excluded = overrideDefaults
            ? new List<PathString>()
            : new()
            {
                "/favicon.ico",
                RedirectPagesKonfigurasjon.Error,
                RedirectPagesKonfigurasjon.Forbidden,
                RedirectPagesKonfigurasjon.LoggedOut,
                RedirectPagesKonfigurasjon.Statuscode,
                "/Account/Login",
                "/Account/Logout"
            };
        if (excludeList.Any())
            excluded.AddRange(excludeList);

        app.UseProtectPaths(
            new ProtectPathsOptions(DeterminePresidingPolicy(), RedirectPagesKonfigurasjon.Forbidden)
            {
                Exclusions = excluded
            });
    }

    public void UseHelseIdProtectedPaths(IApplicationBuilder app) => UseHelseIdProtectedPaths(app, new List<PathString>());

    /// <summary>
    /// Adds Authorization, replaces services.AddAuthorization()
    /// </summary>
    /// <returns></returns>
    public (AuthorizationPolicy AuthPolicy, string PolicyName) AddHelseIdAuthorizationPolicy()
    {
        if (!HelseIdWebKonfigurasjon.AuthUse)
        {
            var noAuthorizationPolicy = new AuthorizationPolicyBuilder().AddRequirements(new NoAuthorizationRequirement()).Build();
            return (noAuthorizationPolicy, Policies.NoAuthorization);
        }

        var authenticatedPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        var hidOrApiPolicy = new AuthorizationPolicyBuilder()
            .Combine(authenticatedPolicy)
            .AddRequirements(new SecurityLevelOrApiRequirement())
            .Build();
        hidOrApiPolicy = authenticatedPolicy;
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

    /// <summary>
    /// Use this to add the HelseId Api access token handling to the app.
    /// </summary>
    public HelseIdWebAuthBuilder AddOutgoingApiServices()
    {
        services.AddAccessTokenManagement();
        services.AddTransient<AuthHeaderHandler>();
        return this;
       
    }

    public HelseIdWebAuthBuilder WithHttpClients()
    {
        foreach (var api in HelseIdWebKonfigurasjon.Apis)
        {
            if (HelseIdWebKonfigurasjon is { AuthUse: true, UseApis: true })
                AddHelseIdApiServices(api);
            else
                AddHelseIdApiServicesNoAuth(api);
        }
        return this;
    }


    private IHttpClientBuilder AddHelseIdApiServices(IApiOutgoingKonfigurasjon api)
    {
        return services.AddUserAccessTokenHttpClient(api.Name, configureClient: client =>
            {
                client.BaseAddress = api.Uri;
                client.Timeout = TimeSpan.FromMinutes(10);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();
    }

    private IHttpClientBuilder AddHelseIdApiServicesNoAuth(IApiOutgoingKonfigurasjon api)
    {
        return services.AddHttpClient(api.Name, client =>
        {
            client.BaseAddress = api.Uri;
            client.Timeout = TimeSpan.FromMinutes(10);
        });
    }


}

public class NoHprApprovals : GodkjenteHprKategoriListe
{
    public NoHprApprovals()
    {
    }
}