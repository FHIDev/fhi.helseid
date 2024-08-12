using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;

namespace Fhi.HelseId.Integration.Tests;

public class ApiTests
{
    private readonly WebApplicationFactory<Program> _factory = new();

    [Test]
    public async Task ValidToken_Returns200Ok()
    {
        var jwt = await TokenCreator.GetHelseIdToken();
        using var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseBody, Is.EqualTo("Hello world!"));
    }

    [Test]
    public async Task InvalidToken_Returns401Unauthorized()
    {
        var jwt = await TokenCreator.GetHelseIdToken();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("api/test");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(responseBody, Is.Empty);
    }
}