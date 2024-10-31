using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Hpr.Core;
using HprServiceReference;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests.Hpr
{
    public class HprServiceTests
    {
        private const int Hprnummer = 123456789;

#pragma warning disable
        private IHprFactory factory;
#pragma warning disable
        private IHPR2ServiceChannel channel;
        private IMemoryCache memoryCache;
        private IHttpContextAccessor httpContextAccessor;
        private ILogger logger;

        private HprService hprService;

        [SetUp]
        public void Setup()
        {
            factory = Substitute.For<IHprFactory>();
            channel = Substitute.For<IHPR2ServiceChannel>();
            httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            factory.ServiceProxy.Returns(channel);
            memoryCache = Substitute.For<IMemoryCache>();
            logger = Substitute.For<ILogger>();

            hprService = new HprService(factory, memoryCache, httpContextAccessor, logger);
        }

        [Test]
        public async Task AtViKanLasteEnPerson()
        {
            var person = new TestLege(Hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await hprService.HentPerson(Hprnummer.ToString());

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.HprNummer, Is.EqualTo(Hprnummer));
                //Assert.That(result.FysiskeAdresser.Length, Is.EqualTo(1)); // TODO: Can't get fysiske adresser from context?
                //Assert.That(result.FysiskeAdresser[0].Gateadresse, Is.EqualTo(person.FysiskeAdresser[0].Gateadresse)); // TODO: Can't get fysiske adresser from context?
            });
        }

        [Test]
        public async Task AtPersonErLege()
        {
            var person = new TestLege(Hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result);
        }

        [Test]
        public async Task AtPersonIkkeErLege()
        {
            var person = StubPersonAnnet.CreateStubPersonAnnet(Hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task AtLegeErSuspendert()
        {
            var person = new TestLege(Hprnummer).Suspender();
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Ignore("Usikker case")]
        [Test]
        public async Task AtLegeHarAktivSuspansjonITillegg()
        {
            var person = new TestLege(Hprnummer).SuspenderTillegg();
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var result = await hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task AtLegeIkkeErAutorisert()
        {
            var person = new TestLege(Hprnummer).EndreTilUgyldig();
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task AtFlereKategorierKanLeggesTil()
        {
            var person = new TestSykePleier(Hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = await hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result);
        }

        [Test]
        public async Task AtFlereGodkjenningerKanLesesFraPerson()
        {
            var person = new TestPersonMedFlereGodkjenninger(Hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);

            bool result = await hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result, "Hprnummer ikke godkjent");

            hprService.LeggTilGodkjenteHelsepersonellKategoriListe(new List<OId9060>
                {Kodekonstanter.OId9060Sykepleier, Kodekonstanter.OId9060Lege});

            var godkjenninger = await hprService.HentGodkjenninger(Hprnummer.ToString());

            Assert.Multiple(() =>
            {
                Assert.That(godkjenninger.Count, Is.EqualTo(2), "Antall godkjenninger ble galt");
                Assert.That(godkjenninger, Does.Contain(Kodekonstanter.OId9060Sykepleier));
                Assert.That(godkjenninger, Does.Contain(Kodekonstanter.OId9060Lege));
                Assert.That(godkjenninger, Does.Not.Contain(Kodekonstanter.OId9060Jordmor));
            });
        }

        [Test]
        public async Task AtViKanLeggeTilKategorierUtenDuplikater()
        {
            var person = new TestPersonMedFlereGodkjenninger(Hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            hprService.LeggTilAlleKategorier();
            var godkjenninger1 = await hprService.HentGodkjenninger(Hprnummer.ToString());
            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var godkjenninger2 = await hprService.HentGodkjenninger(Hprnummer.ToString());

            Assert.That(godkjenninger1.Count, Is.EqualTo(godkjenninger2.Count()));
            Assert.That(godkjenninger2.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task AtViHenterFraCacheNårHprPersonErCachet()
        {
            var person = new TestPersonMedFlereGodkjenninger(Hprnummer);
            channel.HentPersonAsync(Hprnummer, null).Returns(person);
            memoryCache.TryGetValue(Arg.Any<object>(), out Arg.Any<Person>())
                .Returns(true);

            hprService.LeggTilAlleKategorier();
            await hprService.HentGodkjenninger(Hprnummer.ToString());

            channel.DidNotReceive().HentPersonAsync(Hprnummer, null);
            memoryCache.DidNotReceive().Set(Arg.Any<object>(), person);
        }

        [Test]
        public async Task AtViHenterFraServiceNårHprPersonIkkeErCachet()
        {
            var person = new TestPersonMedFlereGodkjenninger(Hprnummer);
            channel.HentPersonAsync(Hprnummer, null).Returns(person);

            hprService.LeggTilAlleKategorier();
            await hprService.HentGodkjenninger(Hprnummer.ToString());

            channel.Received().HentPersonAsync(Hprnummer, null);
            memoryCache.Received().Set(Arg.Any<object>(), person);
        }
    }
}