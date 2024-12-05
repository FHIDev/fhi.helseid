using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Integration.Tests.TestFramework;

namespace Fhi.HelseId.Integration.Tests.HelseId.Web.Tests
{

    public class AuthorizationCodeFlowTests
    {

        [Test]
        [Ignore("//TODO:Next step")]
        public async Task GIVEN_xx_WHEN_xx_THEN()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
                {"HelseIdWebKonfigurasjon:AuthUse", "false" },
                { "HelseIdWebKonfigurasjon:Authority", "https://helseid-sts.test.nhn.no/" }
            };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();

            var appFactory = new WebApplicationFactoryTestHost(testConfiguration, services =>
            {
                services.AddHelseIdWebAuthentication(testConfiguration)
               .UseJwkKeySecretHandler()
               .Build();

            });


            var testClient = appFactory.CreateClient();
            var testResponse = await testClient.GetAsync("/api/test");
            var stringcontent = testResponse.Content.ReadAsStringAsync();

            var getUser = await testClient.GetAsync("/user");

        }


    }
}




