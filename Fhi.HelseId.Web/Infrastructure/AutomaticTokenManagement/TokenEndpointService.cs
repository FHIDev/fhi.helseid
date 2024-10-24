﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fhi.HelseId.Common.ExtensionMethods;
using Fhi.HelseId.Web.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;

/// <summary>
/// Token endpoint service for refreshing access tokens
/// </summary>
public interface ITokenEndpointService
{
    /// <summary>
    /// Performs a request using the refresh_token grant type, normally trough a TokenClient using OpenID Connect / OAuth 2 
    /// </summary>
    /// <param name="refreshToken">An OIDC refresh token</param>
    /// <returns>Result of the refresh attempt</returns>
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
}

/// <summary>
/// Token endpoint service using an underlying TokenClient and OIDC configuration for refreshing access tokens
/// </summary>
public class TokenEndpointService : ITokenEndpointService
{
    private readonly AutomaticTokenManagementOptions managementOptions;
    private readonly IOptionsSnapshot<OpenIdConnectOptions> oidcOptions;
    private readonly IAuthenticationSchemeProvider schemeProvider;
    private readonly HttpClient httpClient;
    private readonly ILogger<TokenEndpointService> logger;
    private readonly IHelseIdSecretHandler? secretHandler;

    public TokenEndpointService(
        IOptions<AutomaticTokenManagementOptions> managementOptions,
        IOptionsSnapshot<OpenIdConnectOptions> oidcOptions,
        IAuthenticationSchemeProvider schemeProvider,
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TokenEndpointService> logger,
        IHelseIdSecretHandler secretHandler)
    {
        logger.LogMember();
        this.secretHandler = secretHandler; //as HelseIdJwkSecretHandler;
        this.managementOptions = managementOptions.Value;
        this.oidcOptions = oidcOptions;
        this.schemeProvider = schemeProvider;
        this.httpClient = httpClient;
        this.logger = logger;
        httpContextAccessor.HttpContext?.Features.Get<AuthorizationCodeReceivedContext>();
    }

    /// <summary>
    /// Performs a token refresh trough a TokenClient using the given OIDC configuration.
    /// </summary>
    /// <param name="refreshToken">An OIDC refresh token</param>
    /// <returns>Result of the refresh attempt</returns>
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var oidcOptions2 = await GetOidcOptionsAsync();

        if (oidcOptions2.ConfigurationManager == null)
        {
            throw new InvalidOperationException("ConfigurationManager cannot be null.");
        }

        var configuration = await oidcOptions2.ConfigurationManager.GetConfigurationAsync(default);
        var clientAssertion = secretHandler?.GenerateClientAssertion;
        logger.LogTrace(
            $"TokenEndPointService:RefreshTokenAsync. TokenEndpointAddress: {configuration.TokenEndpoint} ClientId: {oidcOptions2.ClientId} ClientAssertion: {clientAssertion}");
        var tokenClient = new TokenClient(httpClient, new TokenClientOptions
        {
            Address = configuration.TokenEndpoint,
            ClientId = oidcOptions2.ClientId,
            ClientAssertion = new ClientAssertion
            { Value = clientAssertion, Type = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer },
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
        });
        var response = await tokenClient.RequestRefreshTokenAsync(refreshToken);
        var tokenResponseJson = JsonConvert.SerializeObject(response);
        logger.LogTrace("{class}.{method} : refreshtoken: {json}", nameof(TokenEndpointService), nameof(RefreshTokenAsync), tokenResponseJson);
        if (response.IsError)
        {
            logger.LogError($"TokenEndPointService:RefreshTokenAsync. Error: {response.Error} ErrorDescription: {response.ErrorDescription}");
        }
        return response;
    }

    private async Task<OpenIdConnectOptions> GetOidcOptionsAsync()
    {
        if (string.IsNullOrEmpty(managementOptions.Scheme))
        {
            var scheme = await schemeProvider.GetDefaultChallengeSchemeAsync();

            if (scheme == null)
            {
                throw new InvalidOperationException("No AuthenticationScheme was specified, and there was no DefaultChallengeScheme found.");
            }

            return oidcOptions.Get(scheme.Name);
        }
        return oidcOptions.Get(managementOptions.Scheme);
    }
}