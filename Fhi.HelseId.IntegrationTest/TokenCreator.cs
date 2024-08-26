using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Fhi.ClientCredentialsKeypairs;
using Fhi.HelseId.Integration.Tests.TttClient;

namespace Fhi.HelseId.Integration.Tests;

internal class TokenCreator
{
    private static readonly string[] DefaultScopes = ["fhi:helseid.testing.api/all"];
    private const string HelseIdTttConfigFile =
        "HelseID Configuration 983744516-HelseID TTT-klient.json";
    private const string HelseIdCreateTokenEndpoint =
        "https://helseid-ttt.test.nhn.no/create-test-token";

    internal static async Task<string> GetHelseIdToken()
    {
        var tokenClient = new Client("https://helseid-ttt.test.nhn.no", await CreateHttpClient());
        var body = new TokenRequest()
        {
            GeneralClaimsParameters = new GeneralClaimsParameters() { Scope = DefaultScopes },
            UserClaimsParameters = new UserClaimsParameters(),
            GeneralClaimsParametersGeneration =
                ParametersGeneration._3___GenerateDefaultWithClaimsFromNonEmptyParameterValues,
            UserClaimsParametersGeneration =
                ParametersGeneration._3___GenerateDefaultWithClaimsFromNonEmptyParameterValues,
        };
        var response = await tokenClient.CreateTestTokenAsync(body);
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

        var auth = new AuthenticationService(config);
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
