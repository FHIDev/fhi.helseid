using Fhi.HelseId.Api;

namespace Fhi.HelseId.Integration.Tests.HelseId.Api.Setup
{
    public static class HelseIdApiKonfigurasjonExtensions
    {
        internal static HelseIdApiKonfigurasjon CreateHelseIdApiKonfigurasjon(
            string authority = "https://helseid-sts.test.nhn.no",
            string allowedScopes = "",
            string audience = "fhi:api-access-scope",
            bool allowDPoPTokens = true,
            bool requireDPoPTokens = false)
        {
            var config = new HelseIdApiKonfigurasjon()
            {
                Authority = authority,
                ApiName = audience,
                ApiScope = allowedScopes,
                AuthUse = true,
                UseHttps = true,
                RequireContextIdentity = true,
                AllowDPoPTokens = allowDPoPTokens,
                RequireDPoPTokens = requireDPoPTokens
            };

            return config;
        }
    }
}
