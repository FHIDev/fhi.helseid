using Fhi.TestFramework.NHNTTT.Dtos;

namespace Fhi.TestFramework.NHNTTT;

/// <summary>
/// TODO: Should distinguish between token for app2app (client_credential) and token for end-users (authorization_code with PKCE)
/// </summary>
internal static class TTTTokenRequests
{
    /// <summary>
    /// Generates a basic Id token with pid and userclaims specified
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="scopes"></param>
    /// <param name="pid"></param>
    /// <param name="hprNummer"></param>
    /// <param name="securityLevel"></param>
    /// <returns></returns>
    internal static TokenRequest IdToken(string clientId, ICollection<string> scopes, string? pid = "05898597468", string? hprNummer = "", string? securityLevel = "4")
    {
        return new TokenRequest(clientId)
        {
            SetPidPseudonym = false,
            GeneralClaimsParameters = new GeneralClaimsParameters(scopes),
            GetPersonFromPersontjenesten = true,
            WithoutDefaultUserClaims = true,
            UserClaimsParameters = new UserClaimsParameters()
            {
                HprNumber = hprNummer,
                SecurityLevel = securityLevel,
                Pid = pid,
                Subject = "OCW6BpVN57vnbxBUE8WOOTM9FrkCaBixlD2y8FgYCag="
            }
        };
    }

    internal static TokenRequest DefaultAccessToken(string audience = "fhi:api-name")
    {
        return DefaultAccessToken(["fhi:scope"], audience: audience);
    }

    internal static TokenRequest DefaultAccessToken(
        ICollection<string> scopes,
        string audience = "fhi:api-name",
        bool setPidPseudonym = false,
        bool createDPoPTokenWithDPoPProof = false) =>
        new(audience)
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
