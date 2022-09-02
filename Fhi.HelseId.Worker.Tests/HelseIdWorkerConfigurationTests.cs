namespace Fhi.HelseId.Worker.Tests
{
    public class HelseIdClientKonfigurasjonTests : BaseConfigTests
    {
        [SetUp]
        public void Init() => Init("clientappsettings.test.json");

        [Test]
        public void HelseIdWorkerConfigurationTest()
        {
            var sut = Config?.GetWorkerKonfigurasjon();
            Assert.That(sut, Is.Not.Null, $"Can't load {ConfigFilename}");
            Assert.Multiple(() =>
            {
                Assert.That(sut.Authority, Does.Contain("helseid-sts"), "1");
                Assert.That(sut.AuthUse, Is.True, "2");
                Assert.That(sut.UseHttps, Is.True, "3");
                Assert.That(sut.ClientId, Has.Length.GreaterThan(1));
                Assert.That(sut.AllScopes, Is.Not.Empty, "4");
                Assert.That(sut.Apis, Has.Length.EqualTo(1), "5");
            });
            var api = sut.Apis[0];
            Assert.Multiple(() =>
            {
                Assert.That(api.Name, Is.EqualTo("Personoppslag"));
                Assert.That(api.Url, Is.EqualTo("https://test-personoppslag.fhi.no"));
                Assert.That(api.Scope, Is.EqualTo("fhi:personoppslag/api"));
            });
        }
    }
}