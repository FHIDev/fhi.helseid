using System.Net;
using System.Web;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.TestFramework.Extensions;

namespace Fhi.HelseId.Integration.Tests.HelseId.Web.Tests
{
    /// <summary>
    /// The purpose of these tests is to test the buildt in OIDC authentication scheme in AddHelseIdWebAuthentication
    /// </summary>
    public class AuthenticationTests
    {
        [Test]
        public async Task DefaultHelseIdConfiguration_NoAuthCookieOnApiCall_Return401WithRedirectToIdentityProvider()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create
                .AddDefaultValues()
                .CreateConfigurationRoot();
            var appFactory = new TestWebApplicationFactory(config, services =>
            {
                services.AddHelseIdWebAuthentication(config)
               .Build();
            });

            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location!.Query);
            Assert.That(queryParams["scope"], Is.EqualTo("openid profile helseid://scopes/identity/pid helseid://scopes/identity/pid_pseudonym helseid://scopes/identity/security_level offline_access"));
            Assert.That(queryParams["redirect_uri"], Is.EqualTo("http://localhost/signin-callback"));
        }

        /// <summary>
        /// Test signin callback with token
        /// https://github.com/dotnet/aspnetcore/blob/81a2bab8704d87d324039b42eb1bab0d977f25b8/src/Security/Authentication/test/OpenIdConnect/OpenIdConnectEventTests_Handler.cs
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("Will be implemented later")]
        public async Task DefaultHelseIdConfiguration_InvalidTokenRecieved_()
        {
            var config = HelseIdWebKonfigurasjonBuilder.Create.AddDefaultValues();
            var configRoot = config.CreateConfigurationRoot();
            var appFactory = new TestWebApplicationFactory(configRoot, services =>
            {
                services.AddHelseIdWebAuthentication(configRoot)
               .Build();
            });

            var client = appFactory.CreateClient();
            var response = await client.PostAsync("/signin-callback", null);
        }
    }
}




