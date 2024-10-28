using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Constants;
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
    private readonly AutomaticTokenManagementOptions _managementOptions;
    private readonly IOptionsSnapshot<OpenIdConnectOptions> _oidcOptions;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenEndpointService> _logger;
    private readonly IHelseIdSecretHandler? _secretHandler;

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
        _secretHandler = secretHandler; //as HelseIdJwkSecretHandler;
        _managementOptions = managementOptions.Value;
        _oidcOptions = oidcOptions;
        _schemeProvider = schemeProvider;
        _httpClient = httpClient;
        _logger = logger;
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

        ArgumentNullException.ThrowIfNull(oidcOptions.ConfigurationManager);

        var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);
        var clientAssertion = _secretHandler?.GenerateClientAssertion;

        var tokenEndpointRequest = new OpenIdConnectMessage()
        {
            ClientId = oidcOptions.ClientId,
            GrantType = OpenIdConnectGrantTypes.RefreshToken,
            RefreshToken = refreshToken,
            ClientAssertion = clientAssertion,
            ClientAssertionType = OAuthConstants.JwtBearerClientAssertionType,
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint);
        requestMessage.Content = new FormUrlEncodedContent(tokenEndpointRequest.Parameters);

        try
        {
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var resultMessage = new OpenIdConnectMessage(responseContent);

            var expiresAt = DateTimeOffset.Now + TimeSpan.FromSeconds(int.Parse(resultMessage.ExpiresIn));
            var tokenResponseJson = JsonConvert.SerializeObject(resultMessage);

            return new OidcToken(resultMessage.AccessToken, resultMessage.RefreshToken, expiresAt, tokenResponseJson);
        }
        catch (MsalServiceException ex)
        {
            var message = $"TokenEndPointService:RefreshTokenAsync. Error: {ex.Message}";
            _logger.LogError(ex, message);

            return new OidcToken(ex, message);
        }
    }

    private async Task<OpenIdConnectOptions> GetOidcOptionsAsync()
    {
        if (string.IsNullOrEmpty(_managementOptions.Scheme))
        {
            var scheme = await _schemeProvider.GetDefaultChallengeSchemeAsync();

            if (scheme == null)
            {
                throw new InvalidOperationException("No AuthenticationScheme was specified, and there was no DefaultChallengeScheme found.");
            }

            return _oidcOptions.Get(scheme.Name);
        }
        return _oidcOptions.Get(_managementOptions.Scheme);
    }
}