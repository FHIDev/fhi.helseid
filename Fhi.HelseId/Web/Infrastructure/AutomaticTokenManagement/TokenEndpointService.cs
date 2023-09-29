using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public class TokenEndpointService
    {
        private readonly AutomaticTokenManagementOptions managementOptions;
        private readonly IOptionsSnapshot<OpenIdConnectOptions> oidcOptions;
        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly HttpClient tokenClient;
        private readonly ILogger<TokenEndpointService> logger;
        private readonly AuthorizationCodeReceivedContext? authorizationCodeReceivedContext;

        public TokenEndpointService(
            IOptions<AutomaticTokenManagementOptions> managementOptions,
            IOptionsSnapshot<OpenIdConnectOptions> oidcOptions,
            IAuthenticationSchemeProvider schemeProvider,
            HttpClient tokenClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TokenEndpointService> logger)
        {
            this.managementOptions = managementOptions.Value;
            this.oidcOptions = oidcOptions;
            this.schemeProvider = schemeProvider;
            this.tokenClient = tokenClient;
            this.logger = logger;
            authorizationCodeReceivedContext =
                httpContextAccessor.HttpContext?.Features.Get<AuthorizationCodeReceivedContext>();
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var oidcOptions2 = await GetOidcOptionsAsync();
            var configuration = await oidcOptions2.ConfigurationManager.GetConfigurationAsync(default);
            logger.LogTrace($"TokenEndPointService:RefreshTokenAsync. TokenEndpoint: {configuration.TokenEndpoint} ClientId: {oidcOptions2.ClientId} ClientSecret: {oidcOptions2.ClientSecret}");
            var clientAssertion = authorizationCodeReceivedContext?.TokenEndpointRequest?.ClientAssertion;
            if (clientAssertion == null)
            {
                logger.LogWarning("{class}:{method} - ClientAssertion is null",nameof(TokenEndpointService),nameof(RefreshTokenAsync));
            }
            return await tokenClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = configuration.TokenEndpoint,
                ClientId = oidcOptions2.ClientId,
                ClientAssertion = new ClientAssertion { Value = clientAssertion, Type = IdentityModel.OidcConstants.ClientAssertionTypes.JwtBearer },
                RefreshToken = refreshToken
            });
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
            return await tokenClient.RevokeTokenAsync(new TokenRevocationRequest
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