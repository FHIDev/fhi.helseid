using System.Net;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.Samples.TestServerSetup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Fhi.HelseId.Integration.Tests.TestFramework;

namespace Fhi.HelseId.Samples.TestFramework
{

    internal class TestWebApplicationBuildersamples
    {
        [Test]
        public async Task UsingMinimalProgramBuilder()
        {
            IConfigurationRoot appSettings = CreateConfig();

            var builder = WebApplicationBuilderTestHost.CreateTestBuilder()
            .WithConfiguration(appSettings)
            .WithServices(services =>
            {
                services.AddHelseIdWebAuthentication(appSettings).Build();
            });
            
            var app = builder.BuildApp(app =>
            {
                app.UseRouting();
                app.MapGet("/api/test-endpoint", (context) =>
                {
                    var name = context.User.Identity?.Name;
                    return Task.CompletedTask;
                });
            });
            app.Start();

            using var client = app.GetTestClient();
            var response = await client.GetAsync("api/test-endpoint");
        }


        private static IConfigurationRoot CreateConfig()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
            return testConfiguration;
        }
    }
}
