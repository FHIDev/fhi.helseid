using System.Net;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Fhi.HelseId.Integration.Tests.TestFramework;

namespace Fhi.HelseId.Samples.TestFramework
{
    internal class TestWebApplicationFactorySamples
    {
        [Test]
        public async Task UsingWebTestWebApplicationFactory()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                { "HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var inMemoryConfig = appsettingsConfig.BuildInMemoryConfiguration();

            var appFactory = new WebApplicationFactoryTestHost<Program>(
                inMemoryConfig, 
                services =>
                {
                    services.AddHelseIdWebAuthentication(inMemoryConfig).Build();
                }, 
                app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/api/test", () => "Hello world!");

                    });
            });

            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }

    public partial class Program { }
}
