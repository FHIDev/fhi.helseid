using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using Fhi.ClientCredentialsKeypairs;
using Fhi.HelseId.Api;

namespace Fhi.HelseId.Integration.Tests.Setup
{
    [TestFixture]
    public abstract class IntegrationTest<TProgram>(HelseIdApiKonfigurasjon config)
        where TProgram : class
    {
        public readonly ConfigurableWebApplicationFactory<TProgram> Factory = new(config);
        private Dictionary<TokenType, string> _tokens = new();

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

        public static string GetDirectoryForCaller([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
            => sourceFilePath[..sourceFilePath.LastIndexOf('\\')];

        public HttpClient CreateDirectHttpClient(bool useDpop = true)
        {
            var configString = File.ReadAllText("Fhi.HelseId.Testing.Api.json");
            var config = JsonSerializer.Deserialize<ClientCredentialsConfiguration>(configString)
                ?? throw new Exception("No config found in Fhi.HelseId.Testing.Api.json");

            var client = Factory.CreateClient();
            var handler = BuildProvider(config, useDpop);
            return Factory.CreateDefaultClient(Factory.ClientOptions.BaseAddress, handler);
        }

        private HttpAuthHandler BuildProvider(ClientCredentialsConfiguration config, bool useDpop)
        {
            var apiConfig = new ClientCredentialsKeypairs.Api();
            apiConfig.UseDpop = useDpop;
            var store = new AuthenticationService(config, apiConfig);
            var tokenProvider = new AuthenticationStore(store, config);
            var authHandler = new HttpAuthHandler(tokenProvider);
            return authHandler;
        }

        public HttpClient CreateHttpClient(TokenType tokenType)
        {
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _tokens[tokenType]);
            return client;
        }

        private void ParseTokenToFile(string token, string path, string tokenIdentifier)
        {
            var jwtTokenObj = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var outputJson = new { Header = jwtTokenObj.Header, Payload = jwtTokenObj.Payload };
            string outputJsonString = JsonSerializer.Serialize(
                outputJson,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(
                Path.Combine(path, $"{tokenIdentifier}-contents.json"),
                outputJsonString);
        }
    }
}