using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Fhi.HelseId.Integration.TestFramework.Extensions;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT;
using System.Net;

namespace Fhi.HelseId.Integration.Tests.HelseId.Api.Tests;

public class TokenValidationTests 
{

    private static readonly HelseIdApiKonfigurasjon HelseIdConfig = new()
    {
        Authority = "https://helseid-sts.test.nhn.no/",
        ApiName = "fhi:helseid.testing.api",
        ApiScope = "fhi:helseid.testing.api/all",
        AuthUse = true,
        UseHttps = true,
        RequireContextIdentity = true
    };

    public readonly WebApplicationFactoryTestHost Factory = new(services =>
    {
        services.AddHelseIdApiAuthentication(HelseIdConfig);
        services.AddHelseIdAuthorizationControllers(HelseIdConfig);
    });

    [Test]
    public async Task ExpiredToken_Returns401Unauthorized()
    {
        var testToken = await TTTTokenService.GetHelseIdToken(TTTTokenRequests.DefaultToken().ExpiredToken());
        using var client = Factory.CreateClient().AddBearerAuthorizationHeader(testToken);
        
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        var authHeader = response.Headers.WwwAuthenticate.FirstOrDefault();
        Assert.That(authHeader?.Parameter, Contains.Substring("error=\"invalid_token\", error_description=\"The token expired "));
    }

    [Test]
    public async Task InvalidSigningKey_Returns401Unauthorized()
    {
        var testToken = await TTTTokenService.GetHelseIdToken(TTTTokenRequests.DefaultToken(HelseIdConfig.ApiName).InvalidSigningKey());

        using var client = Factory.CreateClient().AddBearerAuthorizationHeader(testToken);
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var authHeader = response.Headers.WwwAuthenticate.FirstOrDefault();
        Assert.That(authHeader?.Parameter, Contains.Substring("error=\"invalid_token\", error_description=\"The signature key was not found\""));

    }

    [Test]
    public async Task InvalidIssuer_Returns401Unauthorized()
    {
        var testToken = await TTTTokenService.GetHelseIdToken(TTTTokenRequests.DefaultToken().InvalidIssuer());
        using var client = Factory.CreateClient().AddBearerAuthorizationHeader(testToken);
        
        var response = await client.GetAsync("api/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    
}
