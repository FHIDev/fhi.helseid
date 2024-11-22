using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Fhi.HelseId.Integration.Tests.Extensions;
using Fhi.HelseId.Integration.Tests.HelseId.Api.Setup;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT;
using System.Net;

namespace Fhi.HelseId.Integration.Tests.HelseId.Api.AppInitiatedTests;

/// <summary>
/// Purpose of the test is to if multi scope and single scope authorization of configured API scopes works.
/// </summary>
public class ScopeTests
{
  
    [Test]
    public async Task GIVEN_single_scope_configured_WHEN_token_contains_single_scope_THEN_Returns200Ok()
    {
        var scope = "fhi:helseid.testing.api/all";
        HelseIdApiKonfigurasjon config = HelseIdApiKonfigurasjonExtensions.CreateHelseIdApiKonfigurasjon(allowedScopes: scope, audience:scope);
        var testToken = await GetTestToken([scope], config.ApiName);
        using var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(testToken);

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task GIVEN_multiple_scopes_configured_WHEN_token_contains_one_of_the_configured_scopes_THEN_Returns200Ok()
    {
        var audience = "fhi:api-scope/access";
        HelseIdApiKonfigurasjon config = HelseIdApiKonfigurasjonExtensions.CreateHelseIdApiKonfigurasjon(allowedScopes: audience, audience: audience);

        var testToken = await GetTestToken([audience], audience);
        using var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(testToken);

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task GIVEN_multiple_scopes_configured_WHEN_token_contain_all_configured_scopes_THEN_Returns200Ok()
    {
        var audience = "fhi:helseid.testing.api/all";
        HelseIdApiKonfigurasjon config = HelseIdApiKonfigurasjonExtensions.CreateHelseIdApiKonfigurasjon(allowedScopes: $"{audience},fhi:helseid.testing.api/person", audience:audience);
        var testToken = await GetTestToken(["fhi:helseid.testing.api/all", "fhi:helseid.testing.api/person"], audience);

        using var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(testToken);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    /// <summary>
    /// TODO: Should have WWW-Authenticate header with error message
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GIVEN_multiple_scopes_configured_WHEN_token_does_not_contain_any_configured_scopes_THEN_Returns403()
    {
        var audience = "fhi:api-scope/access";
        HelseIdApiKonfigurasjon config = HelseIdApiKonfigurasjonExtensions.CreateHelseIdApiKonfigurasjon(allowedScopes: $"{audience},fhi:helseid.testing.api/person", audience:audience);
        var testToken = await GetTestToken(["fhi:non-valid-scope"], audience);

        using var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(testToken);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

    }

    #region Helpers
    private static TestWebApplicationFactory CreateHelseApiTestFactory(HelseIdApiKonfigurasjon config)
    {
        return new(services =>
        {
            services.AddHelseIdApiAuthentication(config);
            services.AddHelseIdAuthorizationControllers(config);
        });
    }

    private static Task<string> GetTestToken(ICollection<string> scopes, string audience)
    {
        return TTTTokenService.GetHelseIdToken(TTTTokenRequests.DefaultToken(scopes, audience));
    }

    

    #endregion Helpers
}
