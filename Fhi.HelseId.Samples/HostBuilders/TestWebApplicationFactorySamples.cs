using System.Net;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Fhi.TestFramework.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Fhi.HelseId.Common.Identity;

namespace Fhi.HelseId.Samples.TestFramework
{
    internal class TestWebApplicationFactorySamples
    {
        [Test]
        public async Task UsingWebTestWebApplicationFactory_ConfigureServices()
        {
            IConfigurationRoot inMemoryConfig = CreateConfig();

            var appFactory = new WebApplicationFactoryTestHost(
                inMemoryConfig,
                services =>
                {
                    services.AddFakeTestAuthenticationScheme(new List<Claim>
                     {
                         new Claim(IdentityClaims.Name, "Line danser")
                     });
                    services.AddHelseIdWebAuthentication(inMemoryConfig).Build();
                });

            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        private static IConfigurationRoot CreateConfig()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var inMemoryConfig = appsettingsConfig.BuildInMemoryConfiguration();
            return inMemoryConfig;
        }
    }

    public partial class Program { }
}
