using System.Net;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseId.Integration.Tests.tmp
{
    internal class TestSamples
    {
        [Test]
        public async Task Option1_WithWebFactory()
        {
            IConfigurationRoot testConfiguration = CreateConfig();

            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                });
                services.AddHelseIdWebAuthentication(testConfiguration).Build();
            }, app =>
            {
                app.UseAuthentication();
                app.UseRouting();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/api/test", () => "Hello world!");

                });
                app.UseHttpsRedirection();

            });
            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Option2_WithTestServer()
        {
            IConfigurationRoot testConfiguration = CreateConfig();

            var host = TestHostBuilder.Configure(testConfiguration, services =>
            {
                services.AddHelseIdWebAuthentication(testConfiguration).Build();
                services.AddRouting();
                services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                });
            }, app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseRouting();
                app.UseEndpoints(builder =>
                {
                    builder.MapGet("/api/test-endpoint", (context) =>
                    {
                        var name = context.User.Identity?.Name;
                        return Task.CompletedTask;
                    });

                });
            }).CreateTestServer();

            var client = host.CreateClient();
            var response = await client.GetAsync("api/test-endpoint");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Option2_WithMinimalHost()
        {
            IConfigurationRoot testConfiguration = CreateConfig();

            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.WebHost.UseTestServer();
            builder.Configuration.AddConfiguration(testConfiguration);
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            });
            builder.AddHelseIdWebAuthentication().Build();

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRouting();
            app.MapGet("/api/test-endpoint", (context) =>
            {
                var name = context.User.Identity?.Name;
                return Task.CompletedTask;
            });

            app.Start();


            using var client = app.GetTestClient();
            var response = await client.GetAsync("api/test-endpoint");
        }

        private static IConfigurationRoot CreateConfig()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                {"HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
            return testConfiguration;
        }
    }
}
