using Fhi.HelseId.Common;
using Fhi.HelseId.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class ConfigurationExtensions
    {
        public static HelseIdWebKonfigurasjon GetWebKonfigurasjon(this IConfiguration root) =>
            root.GetConfig<HelseIdWebKonfigurasjon>(nameof(HelseIdWebKonfigurasjon)) ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));
    }
}
