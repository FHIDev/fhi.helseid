using Fhi.HelseId.Common;
using Fhi.HelseId.Worker;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class HelseIdWorkerConfigTests : BaseConfigTests
    {
        protected HelseIdWorkerConfigTests(string file, bool test) : base(file, test)
        {
            HelseIdWorkerKonfigurasjonUnderTest = Config.GetSection(nameof(HelseIdWorkerKonfigurasjon))
                .Get<HelseIdWorkerKonfigurasjon>();
        }
        protected HelseIdWorkerKonfigurasjon HelseIdWorkerKonfigurasjonUnderTest { get; }

        protected override HelseIdCommonKonfigurasjon HelseIdKonfigurasjonUnderTest => HelseIdWorkerKonfigurasjonUnderTest;


        [Test]
        public void ThatScopesDontExceedLimit()
        {
            const int maxScopeLengthInHelseId = 600;
            Guard();
            string scopes = string.Join(',', HelseIdWorkerKonfigurasjonUnderTest.AllScopes);
            Assert.That(scopes.Length, Is.LessThanOrEqualTo(maxScopeLengthInHelseId), $"Combined scopes have a maximum length of 600 - including separators, this is {scopes.Length}");
            TestContext.Out.WriteLine($"Total length of scopes is {scopes.Length}");
        }

        [Test]
        public void ThatAuthorityHasTokenSuffix()
        {
            Guard();
            Assert.That(HelseIdWorkerKonfigurasjonUnderTest.Authority, Does.EndWith("connect/token"),
                $"An Worker configuration should have the authority url with suffix 'connect/token', but is {HelseIdWorkerKonfigurasjonUnderTest.Authority}.");
        }

        [Test]
        public void ThatClientIdIsSet()
        {
            Guard();
            Assert.That(HelseIdWorkerKonfigurasjonUnderTest.ClientId.Length, Is.GreaterThan(0), "ClientId is not set");
            TestContext.Out.WriteLine($"ClientId is: {HelseIdWorkerKonfigurasjonUnderTest.ClientId}");
        }

        [Test]
        public void ThatAuthUseIsTrue()
        {
            Guard();
            Assert.That(HelseIdWorkerKonfigurasjonUnderTest.AuthUse, "AuthUse should be true, but is false");
        }


        protected override void Guard()
        {
            Assert.That(HelseIdWorkerKonfigurasjonUnderTest, Is.Not.Null, "No config section named 'HelseIdWorkerKonfigurasjon' found, or derived");
        }
    }
}