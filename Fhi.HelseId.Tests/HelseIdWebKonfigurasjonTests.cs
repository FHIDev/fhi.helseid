using Fhi.HelseId.Common;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    public class HelseIdWebKonfigurasjonTests : BaseConfigTests
    {
        [SetUp]
        public void Init() => base.Init();


        [Test]
        public void HelseIdWebConfigurationTest()
        {
            var sut = Config?.GetWebKonfigurasjon();
            Assert.That(sut, Is.Not.Null, "Can't load appsettings.test.json");
            Assert.Multiple(() =>
            {
                Assert.That(sut!.Authority, Does.Contain("helseid-sts"), "1");
                Assert.That(sut.AuthUse, Is.True, "2");
                Assert.That(sut.UseHttps, Is.True, "3");
                Assert.That(sut.UseHprNumber, Is.False, "4");
                Assert.That(sut.AllScopes.Count, Is.GreaterThan(7), "5");
                Assert.That(sut.Whitelist, Is.Not.Null);
                Assert.That(sut.Whitelist.Count, Is.EqualTo(0));
            });
        }
    }

    
}