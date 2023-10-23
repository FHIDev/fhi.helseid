using System.Net.Http;
using System.Threading.Tasks;
using Fhi.HelseId.Web.Services;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;


namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public class TokenEndpointService
    {
        private readonly AutomaticTokenManagementOptions managementOptions;
        private readonly IOptionsSnapshot<OpenIdConnectOptions> oidcOptions;
        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly HttpClient httpClient;
        private readonly ILogger<TokenEndpointService> logger;
        private readonly AuthorizationCodeReceivedContext? authorizationCodeReceivedContext;
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
            this.secretHandler = secretHandler ; //as HelseIdJwkSecretHandler;
            this.managementOptions = managementOptions.Value;
            this.oidcOptions = oidcOptions;
            this.schemeProvider = schemeProvider;
            this.httpClient = httpClient;
            this.logger = logger;
            authorizationCodeReceivedContext =
                httpContextAccessor.HttpContext?.Features.Get<AuthorizationCodeReceivedContext>();
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var oidcOptions2 = await GetOidcOptionsAsync();
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

        public async Task<TokenRevocationResponse> RevokeTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync();
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);
            logger.LogTrace("TokenEndPoint: RevokeTokenAsync");
            var clientAssertion = authorizationCodeReceivedContext?.TokenEndpointRequest?.ClientAssertion;
            if (clientAssertion == null)
            {
                logger.LogWarning("TokenEndPoint: RevokeTokenAsync. ClientAssertion is null");
            }
            return await httpClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = configuration.AdditionalData[OidcConstants.Discovery.RevocationEndpoint].ToString(),
                ClientId = oidcOptions.ClientId,
                ClientAssertion = new ClientAssertion{ Value=clientAssertion, Type= IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer},
                Token = refreshToken,
                TokenTypeHint = OidcConstants.TokenTypes.RefreshToken
            });
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
}