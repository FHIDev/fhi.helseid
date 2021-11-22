using Fhi.HelseId.Api;
using Fhi.HelseId.Common;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class BaseConfigTests : SetupBaseConfigTests
    {
        private readonly bool test;

        protected bool IsTest => test;

        protected BaseConfigTests(string configFile, bool test) : base(configFile)
        {
            this.test = test;
        }

        protected abstract HelseIdCommonKonfigurasjon HelseIdKonfigurasjonUnderTest { get; }



        [Test]
        public void AuthorityConfigurationTest()
        {
            const string testAuthorityUrl = "https://helseid-sts.test.nhn.no/";
            const string authorityUrl = "https://helseid-sts.nhn.no/";
            Guard();
            var authority = test ? testAuthorityUrl : authorityUrl;
            Assert.That(HelseIdKonfigurasjonUnderTest.Authority, Does.StartWith(authority), $"Wrong authority found: {HelseIdKonfigurasjonUnderTest.Authority}. Only possible with {testAuthorityUrl} for test, and {authorityUrl} for production");

        }
    }
}

