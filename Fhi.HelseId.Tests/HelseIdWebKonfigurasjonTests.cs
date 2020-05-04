using Fhi.HelseId.Web;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    public class HelseIdWebKonfigurasjonTests : BaseConfigTests
    {

        [Test]
        public void HelseIdConfigurationTest()
        {
            var sut = Config?.GetSection(nameof(HelseIdWebKonfigurasjon)).Get<HelseIdWebKonfigurasjon>();
            Assert.That(sut, Is.Not.Null, "Can't load appsettings.test.json");
            Assert.Multiple(() =>
            {
                Assert.That(sut?.Authority, Does.Contain("helseid-sts"), "1");
                Assert.That(sut?.AuthUse, Is.True, "2");
                Assert.That(sut?.UseHttps, Is.True, "3");
                Assert.That(sut?.UseHprNumber, Is.False, "4");
                Assert.That(sut?.AllScopes.Count, Is.GreaterThan(7), "5");
            });
        }
    }
}
