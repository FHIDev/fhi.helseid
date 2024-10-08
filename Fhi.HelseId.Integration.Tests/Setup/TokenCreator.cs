using System.Net.Http.Headers;
using System.Text.Json;
using Fhi.ClientCredentialsKeypairs;
using Fhi.HelseId.Integration.Tests.Setup.TttClient;

namespace Fhi.HelseId.Integration.Tests.Setup;

internal class TokenCreator
{
    private const string HelseIdTttConfigFile =
        "Setup/HelseID Configuration 983744516-HelseID TTT-klient.json";
    private const string HelseIdTttEndpoint = "https://helseid-ttt.test.nhn.no";

    internal static async Task<Dictionary<TokenType, string>> CreateTokens()
    {
        return (
            await Task.WhenAll(
                BuiltInTokens.Tokens.Select(async kv => new
                {
                    kv.Key,
                    Value = await GetHelseIdToken(kv.Value),
                })
            )
        ).ToDictionary(t => t.Key, t => t.Value);
    }

    internal static async Task<string> GetHelseIdToken(TokenRequest request)
    {
        var tokenClient = new Client(HelseIdTttEndpoint, await CreateHttpClient());
        var response = await tokenClient.CreateTestTokenAsync(request);
        return response.SuccessResponse.AccessTokenJwt;
    }

    private static async Task<HttpClient> CreateHttpClient()
    {
        var helseIdTttToken = await RequestHelseIdTttToken();
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            helseIdTttToken
        );
        return httpClient;
    }

    private static async Task<string> RequestHelseIdTttToken()
    {
        var configString = await File.ReadAllTextAsync(HelseIdTttConfigFile);
        var config = JsonSerializer.Deserialize<ClientCredentialsConfiguration>(configString);

        if (config == null)
        {
            throw new Exception(
                $"No valid HelseID configuration was found in {HelseIdTttConfigFile}"
            );
        }

        var apiConfig = new ClientCredentialsKeypairs.Api();
        var auth = new AuthenticationService(config, apiConfig);
        await auth.SetupToken();

        if (string.IsNullOrEmpty(auth.AccessToken))
        {
            throw new Exception(
                "Could not get any valid access token from HelseId for HelseId TTT"
            );
        }

        return auth.AccessToken;
    }
}