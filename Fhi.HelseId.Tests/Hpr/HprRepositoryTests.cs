using System;
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
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result);

        }

        [Test]
        public async Task AtPersonIkkeErLege()
        {
            const int hprnummer = 123456789;
            var person = CreateStubPersonAnnet(hprnummer);
            channel.HentPersonAsync(Arg.Any<int>(), null).Returns(person);

            var repositorySut = new HprService(factory, logger);
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
            repositorySut.LeggTilGodkjenteHelsepersonellkategorier(Kodekonstanter.OId9060Sykepleier);
            var result = await repositorySut.SjekkGodkjenning(hprnummer.ToString());

            Assert.That(result);
        }




        private Person CreateStubPersonAnnet(int hpr)
        {
            var person = new Person
            {
                HPRNummer = hpr,
                FysiskeAdresser =
                    new[] { new FysiskAdresse { Gateadresse = "Hovedgata 23", Postkode = "1234", Poststed = "Oslo" } },
                Godkjenninger = new[]
                {
                    new Godkjenning
                    {
                        Autorisasjon = new Kode {Aktiv = true, },
                        Helsepersonellkategori = new Kode {Verdi= "XX"},
                        Gyldig = new Periode { Fra = DateTime.Today.AddDays(-1), Til=null},
                        Suspensjonsperioder = Array.Empty<Suspensjonsperiode>()
                    },

                }
            };
            return person;
        }

    }

    internal class TestSykePleier : Person
    {
        internal TestSykePleier(int hprnummer)
        {
            HPRNummer = hprnummer;
            FysiskeAdresser =
                new[] { new FysiskAdresse { Gateadresse = "Hovedgata 23", Postkode = "1234", Poststed = "Oslo" } };
            Godkjenninger = new[]
            {
                new Godkjenning
                {
                    Autorisasjon = new Kode {Aktiv = true,},
                    Helsepersonellkategori = new Kode {Verdi = "SP"},
                    Gyldig = new Periode {Fra = DateTime.Today.AddDays(-1), Til = null},
                    Suspensjonsperioder = Array.Empty<Suspensjonsperiode>()
                }

            };
        }
    }


    internal class TestLege : Person
    {
        internal TestLege(int hprnummer)
        {
            HPRNummer = hprnummer;
            FysiskeAdresser =
                new[] { new FysiskAdresse { Gateadresse = "Hovedgata 23", Postkode = "1234", Poststed = "Oslo" } };
            Godkjenninger = new[]
            {
                new Godkjenning
                {
                    Autorisasjon = new Kode {Aktiv = true,},
                    Helsepersonellkategori = new Kode {Verdi = "LE"},
                    Gyldig = new Periode {Fra = DateTime.Today.AddDays(-1), Til = null},
                    Suspensjonsperioder = Array.Empty<Suspensjonsperiode>()
                }

            };
        }

        internal TestLege Suspender()
        {
            var suspensjonsperiode = new Suspensjonsperiode { Periode = new Periode { Fra = DateTime.Today.AddDays(-1), Til = null } };
            Godkjenninger[0].Suspensjonsperioder = new[] { suspensjonsperiode };
            return this;
        }

        internal TestLege EndreTilUgyldig()
        {
            Godkjenninger[0].Gyldig = new Periode { Fra = DateTime.Today.AddDays(1), Til = null };
            return this;
        }

        internal TestLege SuspenderTillegg()
        {
            var godkjenninger = Godkjenninger.ToList();
            var suspensjonsperiode = new Suspensjonsperiode { Periode = new Periode { Fra = DateTime.Today.AddDays(-1), Til = null } };
            godkjenninger.Add(new Godkjenning
            {
                Autorisasjon = new Kode { Aktiv = true, },
                Helsepersonellkategori = new Kode { Verdi = "LE" },
                Gyldig = new Periode { Fra = DateTime.Today.AddDays(-1), Til = null },
                Suspensjonsperioder = new[] { suspensjonsperiode }
            });
            return this;
        }

    }




}
