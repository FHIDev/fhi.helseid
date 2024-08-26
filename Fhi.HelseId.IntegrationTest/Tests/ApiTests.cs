using System.Net;

namespace Fhi.HelseId.Integration.Tests;

public class ApiTests : IntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        base.CreateService("NOT IN USE YET");
    }

    [Test]
    public async Task ValidToken_Returns200Ok()
    {
        using var client = CreateHttpClient("default");
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task ExpiredToken_Returns401Unauthorized()
    {
        using var client = CreateHttpClient("expired");
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(responseBody, Is.Empty);
    }

    [Test]
    public async Task InvalidToken_Returns401Unauthorized()
    {
        using var client = Factory.CreateClient();

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(responseBody, Is.Empty);
    }
}
