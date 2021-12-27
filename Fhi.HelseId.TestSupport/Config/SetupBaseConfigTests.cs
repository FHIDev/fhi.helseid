using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class SetupBaseConfigTests
    {
        public enum AppSettingsUsage
        {
            AppSettingsIsProd,
            AppSettingsIsBaseOnly,
            AppSettingsIsTestWhenDev
        }
        protected string ConfigFile { get; }

        protected bool ConfigIsAppsettings => ConfigFile.Equals("appsettings.json",System.StringComparison.InvariantCultureIgnoreCase);
        protected SetupBaseConfigTests(string configFile,AppSettingsUsage appSettingsUsage)
        {
            ConfigFile = configFile;
            UseOfAppsettings = appSettingsUsage;
            Config = GetIConfigurationRoot(TestContext.CurrentContext.TestDirectory);
        }
        protected IConfigurationRoot Config;
        public AppSettingsUsage UseOfAppsettings { get; }

        protected abstract void Guard();

        private IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            var c = new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile(ConfigFile, optional: true);
            if (!ConfigIsAppsettings)
            {
                c.AddJsonFile(Path.Combine(outputPath, "appsettings.json"));
            }
            return c.Build();
        }
    }
}