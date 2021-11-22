using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class SetupBaseConfigTests
    {
        protected string ConfigFile { get; }

        protected SetupBaseConfigTests(string configFile)
        {
            ConfigFile = configFile;
            Config = GetIConfigurationRoot(TestContext.CurrentContext.TestDirectory);
        }
        protected IConfigurationRoot Config;

        protected abstract void Guard();

        private IConfigurationRoot GetIConfigurationRoot(string outputPath) =>
            new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile(ConfigFile, optional: true)
                .Build();
    }
}