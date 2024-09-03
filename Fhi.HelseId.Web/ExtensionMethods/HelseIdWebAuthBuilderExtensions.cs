using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Fhi.HelseId.Web.ExtensionMethods;

public static class HelseIdWebAuthBuilderExtensions
{
    public static HelseIdWebAuthBuilder AuthBuilder { get; private set; }

    /// <summary>
    /// Use this when working with Minimal app
    /// </summary>
    public static HelseIdWebAuthBuilder AddHelseIdWebAuthentication(this WebApplicationBuilder builder)
    {
        AuthBuilder = new HelseIdWebAuthBuilder(builder.Configuration, builder.Services);
        return AuthBuilder;
    }

    /// <summary>
    /// Use this when working with legacy non-minimal app
    /// </summary>
    public static HelseIdWebAuthBuilder AddHelseIdWebAuthentication(this IServiceCollection services, IConfiguration config)
    {
        AuthBuilder = new HelseIdWebAuthBuilder(config, services);
        return AuthBuilder;
    }

    /// <summary>
    /// The ClientSecret property should contain the Jwk private key as a string
    /// </summary>
    public static HelseIdWebAuthBuilder UseJwkKeySecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = AuthBuilder.HelseIdWebKonfigurasjon.AuthUse ? new HelseIdJwkSecretHandler() : new HelseIdNoAuthorizationSecretHandler();
        return authBuilder;
    }

    /// <summary>
    /// Used when you have the Jwk in a file. The file should contain the Jwk as a string. The ClientSecret property should contain the file name
    /// </summary>
    public static HelseIdWebAuthBuilder UseJwkKeyFileSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = AuthBuilder.HelseIdWebKonfigurasjon.AuthUse ? new HelseIdJwkFileSecretHandler() : new HelseIdNoAuthorizationSecretHandler();
        return authBuilder;
    }
    /// <summary>
    /// For selvbetjening we expect ClientSecret to be a path to a file containing the full downloaded configuration file, including the private key in JWK format
    /// </summary>
    public static HelseIdWebAuthBuilder UseSelvbetjeningFileSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = AuthBuilder.HelseIdWebKonfigurasjon.AuthUse ? new HelseIdSelvbetjeningSecretHandler() : new HelseIdNoAuthorizationSecretHandler();
        return authBuilder;
    }
    /// <summary>
    /// For Azure Key Vault Secret we expect ClientSecret in the format 'name of secret;uri to vault'. For example: 'MySecret;https://your-unique-key-vault-name.vault.azure.net/'
    /// </summary>
    public static HelseIdWebAuthBuilder UseAzureKeyVaultSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = AuthBuilder.HelseIdWebKonfigurasjon.AuthUse ? new HelseIdJwkAzureKeyVaultSecretHandler() : new HelseIdNoAuthorizationSecretHandler();
        return authBuilder;
    }

    /// <summary>
    /// Use when a shared secret key is in the CLientSecret property
    /// </summary>
    public static HelseIdWebAuthBuilder UseSharedSecretHandler(this HelseIdWebAuthBuilder authBuilder)
    {
        authBuilder.SecretHandler = AuthBuilder.HelseIdWebKonfigurasjon.AuthUse ? new HelseIdSharedSecretHandler() : new HelseIdNoAuthorizationSecretHandler();
        return authBuilder;
    }

    /// <summary>
    /// End a fluent series with this to create the authentication handlers. It returns the builder which can be further used later if needed, otherwise ignore the return.
    /// This sets up authentication and authorization services, and adds the controllers. You still need to call app.UseAuthentication() and app.UseAuthorization() to enable the middleware.
    /// </summary>
    public static HelseIdWebAuthBuilder Build(this HelseIdWebAuthBuilder authBuilder, Action<MvcOptions>? configureMvc = null,
        ConfigureAuthentication? configureAuthentication = null)
    {
        authBuilder.AddHelseIdWebAuthentication(configureMvc,configureAuthentication);
        return authBuilder;
    }

    public static void UseHelseIdProtectedPaths(this IApplicationBuilder app) => AuthBuilder.UseHelseIdProtectedPaths(app);
    public static void UseHelseIdProtectedPaths(this IApplicationBuilder app,IReadOnlyCollection<PathString> excludeList, bool overrideDefaults = false) => AuthBuilder.UseHelseIdProtectedPaths(app,excludeList,overrideDefaults);
    
    
}