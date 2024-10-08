using Fhi.HelseId.Api;
using System.Net;

namespace Fhi.HelseId.Integration.Tests.Setup;

public class AllowedDPoPTokenTests : IntegrationTest<Program>
{
    private static readonly HelseIdApiKonfigurasjon HelseIdConfig = new HelseIdApiKonfigurasjon
    {
        Authority = "https://helseid-sts.test.nhn.no/",
        ApiName = "fhi:helseid.testing.api",
        ApiScope = "fhi:helseid.testing.api/all",
        AuthUse = true,
        UseHttps = true,
        RequireContextIdentity = true,
        AllowDPoPTokens = true,
    };

    public AllowedDPoPTokenTests() : base(HelseIdConfig)
    { }

    [Test]
    public async Task ValidDPoPToken_Returns200Ok()
    {
        using var client = CreateDirectHttpClient();
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task BearerToken_Returns200Ok()
    {
        using var client = CreateDirectHttpClient(useDpop: false);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }
}
