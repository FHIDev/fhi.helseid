using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Hpr.Core;
using Fhi.HelseId.Web.Services;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests.Hpr
{
    public class HprServiceTests
    {
        private const string Hprnummer = "123456789";

        private ICurrentUser _currentUser;
        private HprService _hprService;

        [SetUp]
        public void Setup()
        {
            _currentUser = Substitute.For<ICurrentUser>();

            _hprService = new HprService(_currentUser);
        }

        [Test]
        public void AtViKanLasteEnPerson()
        {
            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = _hprService.HentPerson(Hprnummer.ToString());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HprNummer, Is.EqualTo(Hprnummer));
        }

        [Test]
        public void AtPersonErLege()
        {
            _currentUser.ErHprGodkjent.Returns(true);
            _currentUser.HprGodkjenninger.Returns(new List<OId9060> { Kodekonstanter.OId9060Lege });

            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = _hprService.SjekkGodkjenning(Hprnummer);

            Assert.That(result);
        }

        [Test]
        public void AtPersonIkkeErLege()
        {
            _currentUser.ErHprGodkjent.Returns(false);
            _currentUser.HprGodkjenninger.Returns(new List<OId9060>());

            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = _hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result, Is.False);
        }

        [Test]
        public void AtFlereKategorierKanLeggesTil()
        {
            _currentUser.ErHprGodkjent.Returns(true);
            _currentUser.HprGodkjenninger.Returns(new List<OId9060> { Kodekonstanter.OId9060Sykepleier });

            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var result = _hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result);
        }

        [Test]
        public void AtFlereGodkjenningerKanLesesFraPerson()
        {
            _currentUser.ErHprGodkjent.Returns(true);
            _currentUser.HprGodkjenninger.Returns(new List<OId9060> { Kodekonstanter.OId9060Lege, Kodekonstanter.OId9060Sykepleier, Kodekonstanter.OId9060Jordmor });

            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);

            bool result = _hprService.SjekkGodkjenning(Hprnummer.ToString());

            Assert.That(result, "Hprnummer ikke godkjent");

            _hprService.LeggTilGodkjenteHelsepersonellKategoriListe(new List<OId9060>
                {Kodekonstanter.OId9060Sykepleier, Kodekonstanter.OId9060Lege});

            var godkjenninger = _hprService.HentGodkjenninger(Hprnummer.ToString());

            Assert.Multiple(() =>
            {
                Assert.That(godkjenninger.Count, Is.EqualTo(2), "Antall godkjenninger ble galt");
                Assert.That(godkjenninger, Does.Contain(Kodekonstanter.OId9060Sykepleier));
                Assert.That(godkjenninger, Does.Contain(Kodekonstanter.OId9060Lege));
                Assert.That(godkjenninger, Does.Not.Contain(Kodekonstanter.OId9060Jordmor));
            });
        }

        [Test]
        public void AtViKanLeggeTilKategorierUtenDuplikater()
        {
            _currentUser.ErHprGodkjent.Returns(true);
            _currentUser.HprGodkjenninger.Returns(new List<OId9060> { Kodekonstanter.OId9060Lege, Kodekonstanter.OId9060Sykepleier });

            _hprService.LeggTilAlleKategorier();
            var godkjenninger1 = _hprService.HentGodkjenninger(Hprnummer.ToString());
            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Sykepleier);
            _hprService.LeggTilGodkjenteHelsepersonellkategori(Kodekonstanter.OId9060Lege);
            var godkjenninger2 = _hprService.HentGodkjenninger(Hprnummer.ToString());

            Assert.That(godkjenninger1.Count, Is.EqualTo(godkjenninger2.Count()));
            Assert.That(godkjenninger2.Count, Is.EqualTo(2));
        }
    }
}