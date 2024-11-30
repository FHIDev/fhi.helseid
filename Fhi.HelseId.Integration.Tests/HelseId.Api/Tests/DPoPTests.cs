using Fhi.ClientCredentialsKeypairs;
using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Fhi.HelseId.Integration.Tests.HelseId.Api.Setup;
using Fhi.HelseId.Integration.Tests.TestFramework;
using System.Net;
using System.Text.Json;

namespace Fhi.HelseId.Integration.Tests.HelseId.Api.Tests;

/// <summary>
/// Puropse of these tests is to verify that RequireDPoPTokens setting works as intended. It should not accept JWT access_token when DPoP is required. 
/// </summary>
public class DPoPTests 
{
    [Test]
    public async Task ApiCallWithDpopToken_ApiAcceptsBothDpopAndBearer_Returns200Ok()
    {
        var config = HelseIdApiKonfigurasjonExtensions.CreateHelseIdApiKonfigurasjon(
            allowDPoPTokens: true, 
            requireDPoPTokens: false,
            audience: "fhi:helseid.testing.api",
            allowedScopes: "fhi:helseid.testing.api/all");

        var client = CreateDirectHttpClient(config, useDpop: true);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task ApiCallWithBearerToken_ApiAcceptsBothDpopAndBearer_Returns200Ok()
    {
        var config = HelseIdApiKonfigurasjonExtensions.CreateHelseIdApiKonfigurasjon(
            allowDPoPTokens: true, 
            requireDPoPTokens: false,
            audience: "fhi:helseid.testing.api",
            allowedScopes: "fhi:helseid.testing.api/all");

        using var client = CreateDirectHttpClient(config, useDpop: false);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    /// <summary>
    /// TODO: should use WWW-Authenticate header and check for token not validate
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task ApiCallWithBearerToken_ApiAcceptsOnlyDPoP_THEN_Returns401()
    {
        var config = HelseIdApiKonfigurasjonExtensions.CreateHelseIdApiKonfigurasjon(
            allowDPoPTokens: true, 
            requireDPoPTokens: true,
            audience: "fhi:helseid.testing.api",
            allowedScopes: "fhi:helseid.testing.api/all");

        using var client = CreateDirectHttpClient(config, useDpop: false);
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    private  static HttpClient CreateDirectHttpClient(HelseIdApiKonfigurasjon apiConfig, bool useDpop = true)
    {
        var configString = File.ReadAllText("HelseId.Api/Tests/Fhi.HelseId.Testing.Api.json");
        var config = JsonSerializer.Deserialize<ClientCredentialsConfiguration>(configString)
            ?? throw new Exception("No config found in Fhi.HelseId.Testing.Api.json");

        var factory = new TestWebApplicationFactory(services =>
        {
            services.AddHelseIdApiAuthentication(apiConfig);
            services.AddHelseIdAuthorizationControllers(apiConfig);
        });

        var client = factory.CreateClient();
        var handler = BuildProvider(config, useDpop);
        return factory.CreateDefaultClient(factory.ClientOptions.BaseAddress, handler);
    }

    private static HttpAuthHandler BuildProvider(ClientCredentialsConfiguration config, bool useDpop)
    {
        var apiConfig = new ClientCredentialsKeypairs.Api
        {
            UseDpop = useDpop
        };
        var store = new AuthenticationService(config, apiConfig);
        var tokenProvider = new AuthenticationStore(store, config);
        var authHandler = new HttpAuthHandler(tokenProvider);
        return authHandler;
    }

}
