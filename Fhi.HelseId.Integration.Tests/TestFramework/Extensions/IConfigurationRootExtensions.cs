using System.Text.Json;
using Fhi.HelseId.Web;
using Microsoft.Extensions.Configuration;

namespace Fhi.TestFramework.Extensions
{
    internal static class IConfigurationRootExtensions
    {
        internal static IConfigurationRoot BuildInMemoryConfiguration(this Dictionary<string, string?> appsettingsConfig)
        {
            return new ConfigurationBuilder()
            .AddInMemoryCollection(appsettingsConfig)
            .Build();
        }

        internal static IConfigurationRoot CreateConfigurationRoot(this HelseIdWebKonfigurasjon configuration)
        {
            return CreateHelseIdWebConfiguration(
                string.Join(" ", configuration.SecurityLevels),
                configuration.AuthUse,
                configuration.Authority,
                configuration.ClientId,
                JsonSerializer.Serialize(configuration.Scopes));
        }

        private static IConfigurationRoot CreateHelseIdWebConfiguration(string securityLevel = "3", bool authUse = true, string authority = "https://helseid-sts.test.nhn.no/", string clientId = "1224", string scopes = "fhi:api-scope")
        {
            var appsettingsConfig = new Dictionary<string, string?>
                {
                    { "HelseIdWebKonfigurasjon:AuthUse", authUse.ToString() },
                    { "HelseIdWebKonfigurasjon:Authority",  authority},
                    { "HelseIdWebKonfigurasjon:ClientId", clientId },
                    { "HelseIdWebKonfigurasjon:Scopes", scopes },
                    { "HelseIdWebKonfigurasjon:SecurityLevels",  securityLevel}
                };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
            return testConfiguration;
        }

    }
}
