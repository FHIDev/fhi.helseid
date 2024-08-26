using Fhi.HelseId.Integration.Tests.TttClient;

namespace Fhi.HelseId.Integration.Tests;

public abstract class Tokens
{
    internal static TokenRequest DefaultToken =>
        new TokenRequest()
        {
            GeneralClaimsParameters = new GeneralClaimsParameters()
            {
                Scope = ["fhi:helseid.testing.api/all"],
            },

            UserClaimsParameters = new UserClaimsParameters(),
            GeneralClaimsParametersGeneration =
                ParametersGeneration._3___GenerateDefaultWithClaimsFromNonEmptyParameterValues,
            UserClaimsParametersGeneration =
                ParametersGeneration._3___GenerateDefaultWithClaimsFromNonEmptyParameterValues,
        };

    internal static TokenRequest ExpiredToken
    {
        get
        {
            var token = DefaultToken;
            token.ExpirationParameters = new ExpirationParameters()
            {
                SetExpirationTimeAsExpired = true,
            };
            return token;
        }
    }
}
