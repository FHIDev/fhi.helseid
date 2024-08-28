using Fhi.HelseId.Common.ExtensionMethods;
using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Web.DataProtection;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class ConfigurationExtensions
    {
        public static DataProtectionConfig? GetDataProtectionConfig(this IConfiguration configuration)
        {
            var section = configuration.GetSection(nameof(DataProtectionConfig));
            return section.Get<DataProtectionConfig>();
        }

        public static HelseIdWebKonfigurasjon GetWebKonfigurasjon(this IConfiguration root) =>
            root.GetConfig<HelseIdWebKonfigurasjon>(nameof(HelseIdWebKonfigurasjon)) ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));
    }
}
