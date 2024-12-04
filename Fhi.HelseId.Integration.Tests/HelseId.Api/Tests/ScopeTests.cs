using System.Net;
using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Fhi.HelseId.Integration.Tests.HelseId.Api.Setup;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.TestFramework.Extensions;
using Fhi.TestFramework.NHNTTT;
using System.Net;

namespace Fhi.HelseId.Integration.Tests.HelseId.Api.Tests;

/// <summary>
/// Purpose of the test is to if multi scope and single scope authorization of configured API scopes works.
/// </summary>
public class ScopeTests
{
    [Test]
    public async Task SingleScopeIsConfigured_RequestContainsTokenTokenWithTheSingleScope_Returns200Ok()
    {
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: "fhi:helseid.testing.api").WithAllowedScopes("fhi:helseid.testing.api");
        var accessToken = await GetTestToken(["fhi:helseid.testing.api"], config.ApiName);
        
        var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task MultipleScopesIsconfigured_RequestContainsTokenWithOneOfTheConfiguredScopes_Returns200Ok()
    {
        var audience = "fhi:helseid.testing.api";
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: audience).WithAllowedScopes($"{audience}/all,{audience}/person");
        var accessToken = await GetTestToken([$"{audience}/all"], config.ApiName);


        using var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task MultipleScopesIsconfigured_RequestContainsTokenWithAllOfTheConfiguredScopes_Returns200Ok()
    {
        var audience = "fhi:helseid.testing.api";
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: audience).WithAllowedScopes($"{audience}/all,{audience}/person");
        var accessToken = await GetTestToken([$"{audience}/all", $"{audience}/person"], config.ApiName);


        using var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    /// <summary>
    /// This case is also a bit strange since audience will validate ok. The audience of an access_token
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task MultipleScopesIsconfigured_RequestContainsTokenDoesNotContainAnyOfTheConfiguredScopes_Returns403()
    {
        var audience = "fhi:api-scope";
        var config = HelseIdApiKonfigurasjonBuilder.Create.DefaultValues(audience: audience).WithAllowedScopes($"{audience}/all,{audience}/person");
        var accessToken = await GetTestToken(["fhi:non-valid-scope"], audience);

        using var client = CreateHelseApiTestFactory(config).CreateClient().AddBearerAuthorizationHeader(accessToken);
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

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
        return TTTService.GetHelseIdToken(TTTTokenRequests.DefaultAccessToken(scopes, audience));
    }
}
