using Fhi.HelseId.Web;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.Common
{
    public static class ConfigurationExtensions
    {
        public static T GetConfig<T>(this IConfigurationRoot root,string name)
        {
            var section = root.GetSection(name);
            var config =  section.Get<T>();
            return config;
        }

        public static HelseIdWebKonfigurasjon GetWebKonfigurasjon(this IConfigurationRoot root) =>
            root.GetConfig<HelseIdWebKonfigurasjon>(nameof(HelseIdWebKonfigurasjon));
        public static HelseIdApiKonfigurasjon GetApiKonfigurasjon(this IConfigurationRoot root) =>
            root.GetConfig<HelseIdApiKonfigurasjon>(nameof(HelseIdApiKonfigurasjon));

        public static HelseIdWorkerKonfigurasjon GetWorkerKonfigurasjon(this IConfigurationRoot root) =>
            root.GetConfig<HelseIdWorkerKonfigurasjon>(nameof(HelseIdWorkerKonfigurasjon));


    }
}
