using System.Net;

namespace Fhi.HelseId.Integration.Tests;

public class ApiTests : IntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        base.CreateService();
    }

    [Test]
    public async Task ValidToken_Returns200Ok()
    {
        using var client = CreateHttpClient(TokenType.Default);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task ExpiredToken_Returns401Unauthorized()
    {
        using var client = CreateHttpClient(TokenType.Expired);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task InvalidScope_Returns403Forbidden()
    {
        using var client = CreateHttpClient(TokenType.InvalidScope);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task InvalidSigningKey_Returns401Unauthorized()
    {
        using var client = CreateHttpClient(TokenType.InvalidSigningKey);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task InvalidIssuer_Returns401Unauthorized()
    {
        using var client = CreateHttpClient(TokenType.InvalidIssuer);
        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task InvalidToken_Returns401Unauthorized()
    {
        using var client = Factory.CreateClient();

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
