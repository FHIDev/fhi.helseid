using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using ApprovalUtilities.Utilities;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fhi.HelseId.Integration.Tests
{
    [TestFixture]
    public abstract class IntegrationTest
    {
        internal WebApplicationFactory<Program>? _factory;
        internal Dictionary<TokenType, String> _tokens = new();
        public WebApplicationFactory<Program> Factory => _factory!;

        [OneTimeSetUp]
        public async Task CreateTokens()
        {
            _tokens = await TokenCreator.CreateTokens();
            var path = PathUtilities.GetDirectoryForCaller();
            var fullPath = Path.Combine(path, "Tokens");
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            _tokens.ForEach(
                (kvp) =>
                {
                    File.WriteAllText(Path.Combine(fullPath, $"{kvp.Key}.txt"), kvp.Value);
                    ParseTokenToFile(kvp.Value, fullPath, kvp.Key.ToString());
                }
            );
        }

        public void CreateService(string config)
        {
            _factory = new();
        }

        public HttpClient CreateHttpClient(TokenType tokenType)
        {
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _tokens[tokenType]
            );
            return client;
        }

        private void ParseTokenToFile(string token, string path, string tokenIdentifier)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtTokenObj = handler.ReadJwtToken(token);
            var header = jwtTokenObj.Header;
            var payload = jwtTokenObj.Payload;
            string headerJson = JsonSerializer.Serialize(header);
            string payloadJson = JsonSerializer.Serialize(payload);
            var outputJson = new { Header = header, Payload = payload };
            string outputJsonString = JsonSerializer.Serialize(
                outputJson,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(
                Path.Combine(path, $"{tokenIdentifier}-contents.json"),
                outputJsonString
            );
        }
    }
}
