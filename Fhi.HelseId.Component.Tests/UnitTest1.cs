using System.Net;
using System.Net.Http.Headers;
using Fhi.HelseId.Integration.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Fhi.HelseId.Component.Tests
{
    public class Tests
    {
        private readonly WebApplicationFactory<Program> _factory = new();
        private WireMockServer server;

        [SetUp]
        public void Setup()
        {
            server = WireMockServer.Start(port: 9001, useSSL: true);
        }

        [TearDown]
        public void TearDown()
        {
            server.Dispose();
        }

        [Test]
        public async Task Test1()
        {
            var token = JwtTokenGenerator.GenerateToken(100);
            var body = new Dictionary<String, String>
            {
                { "access_token", token },
                { "expires_in", "600" },
                { "scope", "nhn:helseid-test-token-tjeneste/tokengenerering-med-clientid" },
                { "token_type", "bearer" },
            };

            server
                .Given(Request.Create().WithPath("/connect/token"))
                .RespondWith(
                    Response
                        .Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json; charset=UTF-8")
                        .WithBodyAsJson(body)
                );

            var jwt = await TokenCreator.GetHelseIdToken();
            using var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token
            );

            var response = await client.GetAsync("api/test");
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo("Hello world!"));
        }
    }
}
