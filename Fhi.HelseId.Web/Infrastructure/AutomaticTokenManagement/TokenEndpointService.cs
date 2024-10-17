using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fhi.HelseId.Common.ExtensionMethods;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
    Task<OidcToken> RefreshTokenAsync(string refreshToken);
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
    public async Task<OidcToken> RefreshTokenAsync(string refreshToken)
    {
        var oidcOptions = await GetOidcOptionsAsync();
        var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);
        var clientAssertion = secretHandler?.GenerateClientAssertion;

        var tokenEndpointRequest = new OpenIdConnectMessage()
        {
            ClientId = oidcOptions.ClientId,
            GrantType = OpenIdConnectGrantTypes.RefreshToken,
            RefreshToken = refreshToken,
            ClientAssertion = clientAssertion,
            ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer", // TODO: check if there is a const class for this somewhere
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint);
        requestMessage.Content = new FormUrlEncodedContent(tokenEndpointRequest.Parameters);

        try
        {
            var responseMessage = await httpClient.SendAsync(requestMessage);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var resultMessage = new OpenIdConnectMessage(responseContent);

            var expiresOn = DateTimeOffset.Now + TimeSpan.FromSeconds(int.Parse(resultMessage.ExpiresIn));
            var tokenResponseJson = JsonConvert.SerializeObject(resultMessage);

            return new OidcToken(resultMessage.AccessToken, resultMessage.RefreshToken, expiresOn, tokenResponseJson);
        }
        catch (MsalServiceException ex)
        {
            var message = $"TokenEndPointService:RefreshTokenAsync. Error: {ex.Message}";
            logger.LogError(ex, message);

            return new OidcToken(ex, message);
        }
    }

    private async Task<OpenIdConnectOptions> GetOidcOptionsAsync()
    {
        if (string.IsNullOrEmpty(managementOptions.Scheme))
        {
            var scheme = await schemeProvider.GetDefaultChallengeSchemeAsync();
            return oidcOptions.Get(scheme.Name);
        }
        return oidcOptions.Get(managementOptions.Scheme);
    }
}

internal sealed class MsalHttpClientFactoryAdapter(HttpClient httpClient) : IMsalHttpClientFactory
{
    public HttpClient GetHttpClient()
    {
        return httpClient;
    }
}