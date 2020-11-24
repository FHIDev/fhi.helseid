using Fhi.HelseId.Api.DataProtection;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Api.ExtensionMethods
{
    public static class ConfigurationExtensions
    {
        public static DataProtectionConfig GetDataProtectionConfig(this IConfiguration configuration)
        {
            var section = configuration.GetSection(nameof(DataProtectionConfig));
            return section.Get<DataProtectionConfig>();
        }
    }
}
