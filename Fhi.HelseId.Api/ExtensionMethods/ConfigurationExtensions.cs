using Fhi.HelseId.Common;
using Fhi.HelseId.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Api.ExtensionMethods
{
    public static class ConfigurationExtensions
    {
        public static HelseIdApiKonfigurasjon GetApiKonfigurasjon(this IConfiguration root) =>
            root.GetConfig<HelseIdApiKonfigurasjon>(nameof(HelseIdApiKonfigurasjon)) ?? throw new MissingConfigurationException(nameof(HelseIdApiKonfigurasjon));
    }
}
