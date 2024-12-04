using System.Net;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.TestFramework.Extensions;
using Fhi.TestFramework.NHNTTT;

namespace Fhi.HelseId.Integration.Tests.HelseId.Web.Tests
{
    internal class AuthorizationTests
    {
        /// <summary>
        /// Required security level is missing from token
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task SecurityLevelConfigured_AuthenticatedUserCallingAPIWithMissingSecurityLevelInToken_Return403()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.AddDefaultValues().WithSecurityLevel(["3"]);
            var (accessToken, idToken) = await CreateAccessAndIdToken(config.ClientId, config.AllScopes.ToList(), securityLevel: null);

            var testConfiguration = config.CreateConfigurationRoot();
            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                services.AddFakeTestAuthenticationScheme(accessToken, idToken);
                services.AddHelseIdWebAuthentication(testConfiguration).Build();
            });
            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        /// <summary>
        /// Security level in token is higher than configured security level
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task SecurityLevelConfigured_AuthenticatedUserCallingAPIWithSecurityLevelInTokenThatIsHigherThanConfigured_Return200()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.AddDefaultValues().WithSecurityLevel(["3"]);
            var (accessToken, idToken) = await CreateAccessAndIdToken(config.ClientId, config.AllScopes.ToList(), securityLevel: "4");

            var testConfiguration = config.CreateConfigurationRoot();
            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                services.AddFakeTestAuthenticationScheme(accessToken, idToken);
                services.AddHelseIdWebAuthentication(testConfiguration).Build();
            });
            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        /// <summary>
        /// Security level in token is lower than configured security level
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task SecurityLevelConfigured_AuthenticatedUserCallingAPIWithSecurityLevelInTokenThatIsLowerThanConfigured_Return403()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.AddDefaultValues().WithSecurityLevel(["4"]);
            var (accessToken, idToken) = await CreateAccessAndIdToken(config.ClientId, config.AllScopes.ToList(), securityLevel: "2");

            var testConfiguration = config.CreateConfigurationRoot();
            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                services.AddFakeTestAuthenticationScheme(accessToken, idToken);
                services.AddHelseIdWebAuthentication(testConfiguration).Build();
            });
            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        private static async Task<(string AccessToken, string IdToken)> CreateAccessAndIdToken(
          string clientId,
          IEnumerable<string> scopes,
          string audience = "fhi:api",
          string? securityLevel = "4")
        {
            var accessToken = await TTTService.GetHelseIdToken(TTTTokenRequests.DefaultAccessToken(scopes.ToList(), audience));
            var idToken = await TTTService.GetHelseIdToken(TTTTokenRequests.IdToken(clientId, scopes.ToList(), securityLevel: securityLevel));

            return (accessToken, idToken);
        }
    }
}
