using System.Net;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.IntegrationTests.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Integration.Tests.HelseId.Web.Tests
{
    internal partial class AuthorizationTests
    {
       

        [Test]
        public async Task AuthenticatedUserCallingAPI_SecurityLevelConfiguredMissingSecurityLevelFromToken_Return403()
        {
            var accessToken = await TTTTokenService.GetHelseIdToken(TTTTokenRequests.DefaultToken(["fhi-api/access"], "fhi-api"));
            var appsettingsConfig = new Dictionary<string, string?>
                {
                    {"HelseIdWebKonfigurasjon:AuthUse", "true" },
                    { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" },
                    { "HelseIdWebKonfigurasjon:ClientId", "1234" },
                    { "HelseIdWebKonfigurasjon:ApiScope", "fhi-api/access" },
                    { "HelseIdWebKonfigurasjon:SecurityLevels", "[\"3\", \"4\"]" }
                };
                var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();

            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                SetupFakeAuthenticatedUser(services, accessToken);

                //Setup HelseId authentication
                services.AddHelseIdWebAuthentication(testConfiguration)
               .Build();

            });


            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        private static void SetupFakeAuthenticatedUser(IServiceCollection services, string accessToken)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = "TestSignin";
                options.DefaultAuthenticateScheme = "TestSignin";
            }).AddTestAuthentication("TestSignin", accessToken);
        }
    }
}
