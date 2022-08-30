using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Worker;

public interface IAuthenticationService
{
    string AccessToken { get; }
    Task SetupToken();
}

public class AuthenticationService : IAuthenticationService
{
    public HelseIdWorkerKonfigurasjon Config { get; }

    private readonly string jwkPrivateKey;

    public AuthenticationService(HelseIdWorkerKonfigurasjon config, string key)
    {
        Config = config;
        jwkPrivateKey = key;
    }

    public string AccessToken { get; private set; } = "";

    public async Task SetupToken()
    {
        const string scopes =
            "fhi:lmr.grunndata/all fhi:lmr.meldingsformidler/all fhi:lmr.meldingsmottak/all fhi:lmr.utleveringslager/all fhi:lmr.pasientregister/all fhi:lmr.rekvirentregister/all fhi:lmr.varseltjeneste/all fhi:lmr.logging/read fhi:personoppslag/api";
        var c = new HttpClient();
        var cctr = new ClientCredentialsTokenRequest
        {
            Address = Config.Authority,
            ClientId = Config.ClientId,
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            Scope = scopes,
            ClientAssertion = new ClientAssertion
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = BuildClientAssertion(Config.Authority, Config.ClientId)
            }
        };
        var response = await c.RequestClientCredentialsTokenAsync(cctr);
        AccessToken = response.AccessToken;
    }

    private string BuildClientAssertion(string audience, string clientId)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, clientId),
            new(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
            new(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
        };

        var credentials = new JwtSecurityToken(clientId, audience, claims, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(60), GetClientAssertionSigningCredentials());

        var tokenHandler = new JwtSecurityTokenHandler();
        var ret = tokenHandler.WriteToken(credentials);
        return ret;
    }
    private SigningCredentials GetClientAssertionSigningCredentials()
    {
        var securityKey = new JsonWebKey(jwkPrivateKey);
        return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
    }
}

public interface IAuthTokenStore
{
    string GetToken();
}

public class AuthenticationStoreDefault : IAuthTokenStore
{
    readonly string token;

    public AuthenticationStoreDefault(string token)
    {
        this.token = token;
    }
    public string GetToken() => token;
}

public static class GlobalAuthenticationStore
{
    public static IAuthTokenStore? AuthTokenStore { get; private set; }
    public static IAuthenticationService? AuthenticatedService { get; private set; }

    public static void CreateGlobalTokenStore(IAuthTokenStore store, IAuthenticationService authenticatedService)
    {
        AuthTokenStore = store;
        AuthenticatedService = authenticatedService;
    }

}

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthTokenStore authTokenStore;

    public AuthHeaderHandler(IAuthTokenStore? authTokenStore)
    {
        this.authTokenStore = authTokenStore ?? throw new ArgumentNullException(nameof(authTokenStore));
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = authTokenStore.GetToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
