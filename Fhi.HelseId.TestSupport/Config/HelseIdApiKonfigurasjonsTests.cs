using Fhi.HelseId.Common;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using HelseIdApiKonfigurasjon = Fhi.HelseId.Api.HelseIdApiKonfigurasjon;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class HelseIdApiConfigTests : BaseConfigTests
    {
        private readonly bool test;

        protected HelseIdApiConfigTests(string file,bool test, AppSettingsUsage useOfAppsettings) : base(file,test,useOfAppsettings)
        {
            this.test = test;
            HelseIdApiKonfigurasjonUnderTest = Config.GetSection(nameof(HelseIdApiKonfigurasjon))
                .Get<HelseIdApiKonfigurasjon>();
            Guard();
        }
        protected HelseIdApiKonfigurasjon HelseIdApiKonfigurasjonUnderTest { get;  }

        protected override HelseIdCommonKonfigurasjon HelseIdKonfigurasjonUnderTest => HelseIdApiKonfigurasjonUnderTest;


        [Test]
        public void ThatScopesDontExceedLimit()
        {
            const int maxScopeLengthInHelseId = 600;
            Guard();
            string scopes = HelseIdApiKonfigurasjonUnderTest.ApiScope;
            Assert.That(scopes.Length, Is.LessThanOrEqualTo(maxScopeLengthInHelseId),$"Combined scopes in {ConfigFile} have a maximum length of 600 - including separators, this is {scopes.Length}");
        }

        [Test]
        public void ThatAuthorityHasNoSuffix()
        {
            Guard();
            Assert.That(HelseIdApiKonfigurasjonUnderTest.Authority, Does.EndWith(".nhn.no/"),
                $"{ConfigFile}: An API should use the authority url without any suffixes.");
        }

        protected sealed override void Guard()
        {
            Assert.That(HelseIdApiKonfigurasjonUnderTest, Is.Not.Null, $"{ConfigFile}: No config section named 'HelseIdApiKonfigurasjon' found, or derived");
        }

    }
}