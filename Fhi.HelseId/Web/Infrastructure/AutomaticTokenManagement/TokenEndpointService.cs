using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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

        public TokenEndpointService(
            IOptions<AutomaticTokenManagementOptions> managementOptions,
            IOptionsSnapshot<OpenIdConnectOptions> oidcOptions,
            IAuthenticationSchemeProvider schemeProvider,
            HttpClient tokenClient,
            ILogger<TokenEndpointService> logger)
        {
            this.managementOptions = managementOptions.Value;
            this.oidcOptions = oidcOptions;
            this.schemeProvider = schemeProvider;
            this.tokenClient = tokenClient;
            this.logger = logger;
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync();
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);
            logger.LogTrace($"TokenEndPoint: RefreshTokenAsync. TokenEndpoint: {configuration.TokenEndpoint} ClientId: {oidcOptions.ClientId} ClientSecret: {oidcOptions.ClientSecret}");
            return await tokenClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = configuration.TokenEndpoint,

                ClientId = oidcOptions.ClientId,
                //ClientAssertion = oidcOptions.ClientAsser
                // ClientSecret = oidcOptions.ClientSecret,
                RefreshToken = refreshToken
            });
        }

        public async Task<TokenRevocationResponse> RevokeTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync();
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);
            logger.LogTrace("TokenEndPoint: RevokeTokenAsync");
            return await tokenClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = configuration.AdditionalData[OidcConstants.Discovery.RevocationEndpoint].ToString(),
                ClientId = oidcOptions.ClientId,
                ClientSecret = oidcOptions.ClientSecret,
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