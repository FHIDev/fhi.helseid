using System.Net;
using System.Net.Http.Headers;

namespace Fhi.HelseId.Integration.Tests;

public class ApiTests : IntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        base.createService("NOT IN USE YET");
    }

    [Test]
    public async Task ValidToken_Returns200Ok()
    {
        using var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _token
        );

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task InvalidToken_Returns401Unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(responseBody, Is.Empty);
    }
}
