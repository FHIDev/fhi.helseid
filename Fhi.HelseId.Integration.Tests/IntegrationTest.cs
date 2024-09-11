using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fhi.HelseId.Integration.Tests.Setup
{
    [TestFixture]
    public abstract class IntegrationTest<TProgram>
        where TProgram : class
    {
        public readonly WebApplicationFactory<TProgram> Factory = new();
        internal Dictionary<TokenType, String> _tokens = new();

        [OneTimeSetUp]
        public async Task CreateTokens()
        {
            _tokens = await TokenCreator.CreateTokens();
            var fullPath = Path.Combine(GetDirectoryForCaller(), "Tokens");
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            foreach (var entry in _tokens)
            {
                File.WriteAllText(Path.Combine(fullPath, $"{entry.Key}.txt"), entry.Value);
                ParseTokenToFile(entry.Value, fullPath, entry.Key.ToString());
            }
        }

        public static string GetDirectoryForCaller(
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = ""
        ) => sourceFilePath[..sourceFilePath.LastIndexOf('\\')];

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
            var jwtTokenObj = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var outputJson = new { Header = jwtTokenObj.Header, Payload = jwtTokenObj.Payload };
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
