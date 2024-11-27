
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Integration.Tests.TestFramework
{
    internal static class ConfigurationExtension
    {
        internal static IConfigurationRoot BuildInMemoryConfiguration(this Dictionary<string, string?> appsettingsConfig)
        {
            return new ConfigurationBuilder()
            .AddInMemoryCollection(appsettingsConfig)
            .Build();
        }
    }
}
