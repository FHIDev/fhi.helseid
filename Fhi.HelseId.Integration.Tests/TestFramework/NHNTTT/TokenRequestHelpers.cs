using Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT.Dtos;

namespace Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT;

/// <summary>
/// TODO: Should distinguish between token for app2app (client_credential) and token for end-users (authorization_code with PKCE)
/// </summary>
internal static class TTTTokenRequests
{

    internal static TokenRequest DefaultToken(string audience = "fhi:api-name")
    {
        return DefaultToken(["fhi:scope"], audience: audience);
    }


    internal static TokenRequest DefaultToken(
        ICollection<string> scopes,
        string audience = "fhi:api-name",
        bool setPidPseudonym = false,
        bool createDPoPTokenWithDPoPProof = false) =>
        new TokenRequest(audience)
        {
            SetPidPseudonym = setPidPseudonym,
            CreateDPoPTokenWithDPoPProof = createDPoPTokenWithDPoPProof,
            GeneralClaimsParameters = new GeneralClaimsParameters(scopes),
            ClientClaimsParametersGeneration = ParametersGeneration.GenerateDefaultWithClaimsFromNonEmptyParameterValues
        };

    internal static TokenRequest ExpiredToken(this TokenRequest tokenRequest)
    {
        tokenRequest.ExpirationParameters = new ExpirationParameters()
        {
            SetExpirationTimeAsExpired = true
        };
        return tokenRequest;
    }


    internal static TokenRequest InvalidApiScopeToken(this TokenRequest tokenRequest)
    {
        tokenRequest.GeneralClaimsParameters = new GeneralClaimsParameters(["fhi:helseid.testing.api/some"], default, default, default, default, default, null, default, default, default, default, default, default);
        return tokenRequest;
    }

    internal static TokenRequest InvalidSigningKey(this TokenRequest tokenRequest)
    {
        tokenRequest.SignJwtWithInvalidSigningKey = true;
        return tokenRequest;
    }

    internal static TokenRequest InvalidIssuer(this TokenRequest tokenRequest)
    {
        tokenRequest.SetInvalidIssuer = true;
        return tokenRequest;
    }

}
