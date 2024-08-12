using Fhi.ClientCredentialsKeypairs;
using Fhi.HelseId.Integration.Tests.TestTokenModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fhi.HelseId.Integration.Tests;

internal class TokenCreator
{
    private static readonly string[] DefaultScopes = ["fhi:helseid.testing.api/all"];
    private const string HelseIdTttConfigFile = "HelseID Configuration 983744516-HelseID TTT-klient.json";
    private const string HelseIdCreateTokenEndpoint = "https://helseid-ttt.test.nhn.no/create-test-token";

    internal static async Task<string> GetHelseIdToken()
    {
        var helseIdTttToken = await RequestHelseIdTttToken();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", helseIdTttToken);

        var request = new TokenRequest()
        {
            GeneralClaimsParameters = new()
            {
                Scope = DefaultScopes
            },
            UserClaimsParameters = new()
            {

            },
            GeneralClaimsParametersGeneration = ParametersGeneration.GenerateDefaultWithClaimsFromNonEmptyParameterValues,
            UserClaimsParametersGeneration = ParametersGeneration.GenerateDefaultWithClaimsFromNonEmptyParameterValues
        };

        var response = await client.PostAsJsonAsync(HelseIdCreateTokenEndpoint, request);
        var responseContent = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (string.IsNullOrEmpty(responseContent?.SuccessResponse?.AccessTokenJwt))
        {
            throw new Exception("HelseId TTT did not return a valid Jwt token");
        }

        return responseContent.SuccessResponse.AccessTokenJwt;
    }

    private static async Task<string> RequestHelseIdTttToken()
    {
        var configString = await File.ReadAllTextAsync(HelseIdTttConfigFile);
        var config = JsonSerializer.Deserialize<ClientCredentialsConfiguration>(configString);

        if (config == null)
        {
            throw new Exception($"No valid HelseID configuration was found in {HelseIdTttConfigFile}");
        }

        var auth = new AuthenticationService(config);
        await auth.SetupToken();

        if (string.IsNullOrEmpty(auth.AccessToken))
        {
            throw new Exception("Could not get any valid access token from HelseId for HelseId TTT");
        }

        return auth.AccessToken;
    }
}
