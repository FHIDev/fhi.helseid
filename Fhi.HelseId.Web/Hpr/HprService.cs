using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Web.Hpr.Core;
using Fhi.HelseId.Web.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    public interface IHprService
    {
        bool SjekkGodkjenning(string hprnummer);
        HprPerson HentPerson(string hprnummer);

        /// <summary>
        /// Sjekker om personen har gyldig aktiv autorisasjon som en av de godkjente kategoriene.  
        /// </summary>
        bool ErGyldig(HprPerson person);

        IHprService LeggTilGodkjenteHelsepersonellkategori(OId9060 ny);
        IHprService LeggTilGodkjenteHelsepersonellkategorier(IGodkjenteHprKategoriListe liste);
        bool ErGyldigForKategorier(HprPerson person, params OId9060[] koder);
        List<OId9060> GodkjenteHelsepersonellkategorier { get; }
        IEnumerable<OId9060> HentGodkjenninger(string hprnummer);
        IEnumerable<OId9060> HentGodkjenninger(HprPerson person);
        IHprService LeggTilAlleKategorier();
    }

    public class HprService : IHprService
    {
        private readonly ICurrentUser _currentUser;
        private readonly ILogger _logger;

        public List<OId9060> GodkjenteHelsepersonellkategorier { get; }

        const string HprnummerAdmin = "000000000";

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

        public bool SjekkGodkjenning(string hprnummer)
        {
            if (hprnummer == HprnummerAdmin)
                return true;
            var person = HentPerson(hprnummer);

            var filteredGodkjenninger = HentGodkjenninger(person);

            return filteredGodkjenninger.Any();
        }

        public HprPerson HentPerson(string hprnummer)
        {
            var person = new HprPerson()
            {
                HprNummer = hprnummer,
                ErHprGodkjent = _currentUser.ErHprGodkjent,
                HprGodkjenninger = _currentUser.HprGodkjenninger
            };

            return person;
        }

        /// <summary>
        /// Sjekker om personen har gyldig aktiv autorisasjon som en av de godkjente kategoriene. 
        /// </summary>
        public bool ErGyldig(HprPerson person) => ErGyldigForKategorier(person, GodkjenteHelsepersonellkategorier.ToArray());

        public bool ErGyldigForKategorier(HprPerson person, params OId9060[] koder)
        {
            var filteredGodkjenninger = person.HprGodkjenninger
                .Where(personGodkjenninger => koder
                    .FirstOrDefault(systemGodkjenninger => systemGodkjenninger.Value == personGodkjenninger.Value) != null);

            return filteredGodkjenninger.Any();
        }

        public IEnumerable<OId9060> HentGodkjenninger(string hprnummer)
        {
            var person = HentPerson(hprnummer);
            return HentGodkjenninger(person);
        }

        public IEnumerable<OId9060> HentGodkjenninger(HprPerson person)
        {
            var filteredGodkjenninger = person.HprGodkjenninger
                .Where(personGodkjenninger => GodkjenteHelsepersonellkategorier
                    .FirstOrDefault(systemGodkjenninger => systemGodkjenninger.Value == personGodkjenninger.Value) != null);

            return filteredGodkjenninger;
        }
    }
}