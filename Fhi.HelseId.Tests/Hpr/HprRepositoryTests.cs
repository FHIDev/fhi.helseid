using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Hpr.Core;
using HprServiceReference;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests.Hpr
{
    public class HprRepositoryTests
    {
#pragma warning disable
        private IHprFactory factory;
#pragma warning disable
        private IHPR2ServiceChannel channel;
        private ILogger logger;

        [SetUp]
        public void Setup()
        {
            factory = Substitute.For<IHprFactory>();
            channel = Substitute.For<IHPR2ServiceChannel>();
            factory.ServiceProxy.Returns(channel);
            logger = Substitute.For<ILogger>();
        }

        [Test]
        public async Task AtViKanLasteEnPerson()
        {
            const int hprnummer = 123456789;
            var person = new TestLege(hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await repositorySut.HentPerson(hprnummer.ToString());

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.HPRNummer, Is.EqualTo(hprnummer));
                Assert.That(result.FysiskeAdresser.Length, Is.EqualTo(1));
                Assert.That(result.FysiskeAdresser[0].Gateadresse, Is.EqualTo(person.FysiskeAdresser[0].Gateadresse));
            });
        }

        [Test]
        public async Task AtPersonErLege()
        {
            const int hprnummer = 123456789;
            var person = new TestLege(hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result);
        }

        [Test]
        public async Task AtPersonIkkeErLege()
        {
            const int hprnummer = 123456789;
            var person = StubPersonAnnet.CreateStubPersonAnnet(hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task AtLegeErSuspendert()
        {
            const int hprnummer = 123456789;
            var person = new TestLege(hprnummer).Suspender();
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Ignore("Usikker case")]
        [Test]
        public async Task AtLegeHarAktivSuspansjonITillegg()
        {
            const int hprnummer = 123456789;
            var person = new TestLege(hprnummer).SuspenderTillegg();
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result, Is.False);
        }


        [Test]
        public async Task AtLegeIkkeErAutorisert()
        {
            const int hprnummer = 123456789;
            var person = new TestLege(hprnummer).EndreTilUgyldig();
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task AtFlereKategorierKanLeggesTil()
        {
            const int hprnummer = 123456789;
            var person = new TestSykePleier(hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result);
        }

        [Test]
        public async Task AtFLereGodkjenningerKanLesesFraPerson()
        {
            const int hprnummer = 123456789;
            var person = new TestPersonMedFlereGodkjenninger(hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            repositorySut.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);

            bool result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result, "Hprnummer ikke godkjent");

            var godkjenninger = await repositorySut.HentGodkjenninger(hprnummer.ToString(), Kodekonstanter.OId9060Sykepleier, Kodekonstanter.OId9060Lege);

            Assert.Multiple(() =>
            {
                Assert.That(godkjenninger.Count, Is.EqualTo(2), "Antall godkjenninger ble galt");
                Assert.That(godkjenninger, Does.Contain(Kodekonstanter.OId9060Sykepleier));
                Assert.That(godkjenninger, Does.Contain(Kodekonstanter.OId9060Lege));
                Assert.That(godkjenninger, Does.Not.Contain(Kodekonstanter.OId9060Jordmor));
            });
        }
    }
}
