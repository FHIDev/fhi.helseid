using Fhi.HelseId.Web.Hpr.Core;
using Fhi.HelseId.Web.Services;
using HprServiceReference;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fhi.HelseId.Web.Hpr
{
    public interface IHprService
    {
        Task<bool> SjekkGodkjenning(string hprnummer);
        Task<HprPerson?> HentPerson(string hprnummer);

        /// <summary>
        /// Sjekker om personen har gyldig aktiv autorisasjon som en av de godkjente kategoriene.  
        /// </summary>
        bool ErGyldig(Person person);

        IHprService LeggTilGodkjenteHelsepersonellkategori(OId9060 ny);
        IHprService LeggTilGodkjenteHelsepersonellkategorier(IGodkjenteHprKategoriListe liste);
        bool ErGyldigForKategorier(Person person, params OId9060[] koder);
        string LastErrorMessage { get; }
        List<OId9060> GodkjenteHelsepersonellkategorier { get; }
        Task<IEnumerable<OId9060>> HentGodkjenninger(string hprnummer);
        IEnumerable<OId9060> HentGodkjenninger(Person? person);
        IHprService LeggTilAlleKategorier();
    }

    public class HprService : IHprService
    {
        private readonly ICurrentUser _currentUser;
        private readonly ILogger _logger;

        public List<OId9060> GodkjenteHelsepersonellkategorier { get; }

        const string HprnummerAdmin = "000000000";
        public string LastErrorMessage { get; private set; } = "";

        public HprService(ICurrentUser currentUser, ILogger<HprService> logger)
        {
            _logger = logger;
            GodkjenteHelsepersonellkategorier = new List<OId9060>();
            _currentUser = currentUser;
        }

        public IHprService LeggTilGodkjenteHelsepersonellkategorier(IGodkjenteHprKategoriListe liste)
        {
            LeggTilGodkjenteHelsepersonellKategoriListe(liste.Godkjenninger);
            return this;
        }

        public IHprService LeggTilGodkjenteHelsepersonellKategoriListe(IEnumerable<OId9060> liste)
        {
            foreach (var godkjent in liste)
                LeggTilGodkjenteHelsepersonellkategori(godkjent);
            return this;
        }

        public IHprService LeggTilGodkjenteHelsepersonellkategori(OId9060 ny)
        {
            GodkjenteHelsepersonellkategorier.Add(ny);
            return this;
        }

        public IHprService LeggTilAlleKategorier()
        {
            LeggTilGodkjenteHelsepersonellKategoriListe(Kodekonstanter.KodeList);
            return this;
        }

        public async Task<bool> SjekkGodkjenning(string hprnummer)
        {
            if (hprnummer == HprnummerAdmin)
                return true;
            var person = await HentPerson(hprnummer);

            // TODO: Skal denne også filtreres? Eller burde denne ligge i HprPerson?

            var filteredGodkjenninger = person.HprGodkjenninger
                .Where(personGodkjenninger => GodkjenteHelsepersonellkategorier
                    .FirstOrDefault(systemGodkjenninger => systemGodkjenninger.Value == personGodkjenninger.Value) != null);

            return filteredGodkjenninger.Any();
        }

        public async Task<HprPerson?> HentPerson(string hprnummer)
        {
            var personFraRegister = await HentFraHprRegister(hprnummer);

            var cacheTidISekunder = 600;
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheTidISekunder));

            _logger.LogDebug("Person med Hpr-nummer {HprNummer} hentet fra register og lagt til i cache i {CacheTid} sekunder", hprnummer, cacheTidISekunder);

            return personFraRegister;
        }

        private async Task<HprPerson?> HentFraHprRegister(string hprnummer)
        {
            try
            {
                var person = new HprPerson()
                {
                    HprNummer = hprnummer,
                    ErHprGodkjent = _currentUser.ErHprGodkjent,
                    HprGodkjenninger = _currentUser.HprGodkjenninger
                };

                return person;
            }
            catch (System.ServiceModel.CommunicationException e)
            {
                var msg = "CommunicationException i aksess til Hpr register. " + e;
                LastErrorMessage = msg;
                _logger.LogError(e, msg);
                return null;
            }
#pragma warning disable 168
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore 168
            {
                //Hvis ekstern service kaster exception returneres null. Eksemplvis mottar vi også en exception hvis fnr ikke finnes.
                const string msg = "Feil i aksess til Hpr register. (Obs: Mottar også en exception hvis fnr ikke finnes)";
                LastErrorMessage = msg;
                _logger.LogError(e, msg);
                return null;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Sjekker om personen har gyldig aktiv autorisasjon som en av de godkjente kategoriene. 
        /// </summary>
        public bool ErGyldig(Person person) => ErGyldigForKategorier(person, GodkjenteHelsepersonellkategorier.ToArray());

        public bool ErGyldigForKategorier(Person person, params OId9060[] koder)
        {
            if (person == null)
                return false;
            return person.Godkjenninger.Any(g => ErAktivGodkjenning(g, koder));
        }

        private bool ErAktivGodkjenning(Godkjenning g, params OId9060[] koder)
        {
            return koder.Select(x => x.ToString()).Contains(g.Helsepersonellkategori.Verdi)
                   && g.Gyldig.Aktiv()
                   && !g.Suspensjonsperioder.Any(s => s.Periode.Aktiv());
        }

        public async Task<IEnumerable<OId9060>> HentGodkjenninger(string hprnummer)
        {
            var person = await HentPerson(hprnummer);

            // TODO: Skal denne også filtreres?

            var filteredGodkjenninger = person.HprGodkjenninger
                .Where(personGodkjenninger => GodkjenteHelsepersonellkategorier
                    .FirstOrDefault(systemGodkjenninger => systemGodkjenninger.Value == personGodkjenninger.Value) != null);

            return filteredGodkjenninger;
        }

        public IEnumerable<OId9060> HentGodkjenninger(Person? person)
        {
            if (person == null)
                return new List<OId9060>();
            var godkjenninger =
                person.Godkjenninger.Where(o => ErAktivGodkjenning(o, GodkjenteHelsepersonellkategorier.ToArray()));
            return Kodekonstanter.KodeList.Where(o =>
                godkjenninger.FirstOrDefault(x => x.Helsepersonellkategori.Verdi == o.Value) != null);
        }
    }
}