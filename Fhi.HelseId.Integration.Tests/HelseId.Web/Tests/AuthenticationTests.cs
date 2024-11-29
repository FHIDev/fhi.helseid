using System.Net;
using System.Web;
using Fhi.HelseId.Integration.Tests.TestFramework;
using Fhi.HelseId.Web.ExtensionMethods;

namespace Fhi.HelseId.Integration.Tests.HelseId.Web.Tests
{

    public class AuthenticationTests
    {
        [Test]
        public async Task NoAuthCookieOnApiCall_DefaultHelseIdConfiguration_Return401WithRedirectToIdentityProvider()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                {"HelseIdWebKonfigurasjon:AuthUse", "true" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" },
                { "HelseIdWebKonfigurasjon:ClientId", "1234" },
            };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();

            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                services.AddHelseIdWebAuthentication(testConfiguration)
               .Build();
            });


            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            var queryParams = HttpUtility.ParseQueryString(response.Headers.Location is not null ? response.Headers.Location.Query : string.Empty );
            Assert.That(queryParams["scope"], Is.EqualTo("openid profile helseid://scopes/identity/pid helseid://scopes/identity/pid_pseudonym helseid://scopes/identity/security_level offline_access"));
            Assert.That(queryParams["redirect_uri"], Is.EqualTo("http://localhost/signin-callback"));
        }


        /// <summary>
        /// https://github.com/dotnet/aspnetcore/blob/81a2bab8704d87d324039b42eb1bab0d977f25b8/src/Security/Authentication/test/OpenIdConnect/OpenIdConnectEventTests_Handler.cs
        /// </summary>
        /// <returns></returns>
        [Test]
        [Ignore("Will be implemented later")]
        public async Task TokenRecivedOnRequest_XX()
        {

            var appsettingsConfig = new Dictionary<string, string?>
            {
                {"HelseIdWebKonfigurasjon:AuthUse", "true" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" },
                { "HelseIdWebKonfigurasjon:ClientId", "1234" },
            };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();

            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {

                services.AddHelseIdWebAuthentication(testConfiguration)
               .Build();

                //services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureOidcOptions>();

            });


            var testClient = appFactory.CreateClient();
            var testResponse = await testClient.GetAsync("/api/test");
            var stringcontent = testResponse.Content.ReadAsStringAsync();

        }


    }

}




