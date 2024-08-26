using Fhi.HelseId.Integration.Tests.TttClient;

namespace Fhi.HelseId.Integration.Tests;

public enum TokenType
{
    Default,
    Expired,
    InvalidScope,
}

public abstract class BuiltInTokens
{
    public static Dictionary<TokenType, TokenRequest> Tokens =>
        new()
        {
            { TokenType.Default, DefaultToken },
            { TokenType.Expired, ExpiredToken },
            { TokenType.InvalidScope, InvalidApiScopeToken },
        };

    private static TokenRequest DefaultToken =>
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

    private static TokenRequest ExpiredToken
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

    private static TokenRequest InvalidApiScopeToken
    {
        get
        {
            var token = DefaultToken;
            token.GeneralClaimsParameters = new GeneralClaimsParameters()
            {
                Scope = ["fhi:helseid.testing.api/some"],
            };
            return token;
        }
    }
}
