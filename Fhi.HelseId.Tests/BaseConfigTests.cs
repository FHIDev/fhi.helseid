using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    public abstract class BaseConfigTests
    {
        protected IConfigurationRoot? Config { get; private set; }

        [SetUp]
        public void Init()
        {
            Config = GetIConfigurationRoot(TestContext.CurrentContext.TestDirectory);
        }


        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.test.json", optional: true)
                .Build();
        }
    }
}