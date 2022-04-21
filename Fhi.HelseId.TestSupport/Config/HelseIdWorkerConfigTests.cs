using Fhi.HelseId.Common;
using Fhi.HelseId.Worker;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{

    public abstract class HelseIdClientBased : BaseConfigTests
    {
        protected HelseIdClientBased(string configFile, bool test, AppSettingsUsage useOfAppsettings) : base(configFile, test,useOfAppsettings)
        {
        }

        protected abstract HelseIdClientKonfigurasjon HelseIdClientKonfigurasjon { get;}

        [Test]
        public void ThatClientIdIsSet()
        {
            Guard();
            Assert.That(HelseIdClientKonfigurasjon.ClientId.Length, Is.GreaterThan(0), $"{ConfigFile}: ClientId is not set");
            TestContext.Out.WriteLine($"ClientId is: {HelseIdClientKonfigurasjon.ClientId}");
        }

        [Test]
        public void ThatScopesDontExceedLimit()
        {
            const int maxScopeLengthInHelseId = 600;
            Guard();
            string scopes = string.Join(',', HelseIdClientKonfigurasjon.AllScopes);
            Assert.That(scopes.Length, Is.LessThanOrEqualTo(maxScopeLengthInHelseId), $"{ConfigFile}: Combined scopes have a maximum length of 600 - including separators, this is {scopes.Length}");
            TestContext.Out.WriteLine($"{ConfigFile}: Total length of scopes is {scopes.Length}");
        }

        [Test]
        public void ThatAuthUseIsTrue()
        {
            Guard();
            Assert.That(HelseIdClientKonfigurasjon.AuthUse, $"{ConfigFile} AuthUse should be true, but is false");
        }

    }



    public abstract class HelseIdWorkerConfigTests : HelseIdClientBased
    {
        protected HelseIdWorkerConfigTests(string file, bool test, AppSettingsUsage useOfAppsettings) : base(file, test,useOfAppsettings)
        {
            HelseIdWorkerKonfigurasjonUnderTest = Config.GetSection(nameof(HelseIdWorkerKonfigurasjon))
                .Get<HelseIdWorkerKonfigurasjon>();
        }
        protected HelseIdWorkerKonfigurasjon HelseIdWorkerKonfigurasjonUnderTest { get; }

        protected override HelseIdCommonKonfigurasjon HelseIdKonfigurasjonUnderTest => HelseIdWorkerKonfigurasjonUnderTest;

        protected override HelseIdClientKonfigurasjon HelseIdClientKonfigurasjon => HelseIdWorkerKonfigurasjonUnderTest;

        

        [Test]
        public void ThatAuthorityHasTokenSuffix()
        {
            Guard();
            Assert.That(HelseIdWorkerKonfigurasjonUnderTest.Authority, Does.EndWith("connect/token"),
                $"{ConfigFile}: An Worker configuration should have the authority url with suffix 'connect/token', but is {HelseIdWorkerKonfigurasjonUnderTest.Authority}.");
        }

        protected override void Guard()
        {
            Assert.That(HelseIdWorkerKonfigurasjonUnderTest, Is.Not.Null, $"{ConfigFile}: No config section named 'HelseIdWorkerKonfigurasjon' found, or derived");
        }
    }


}