using System;
using System.Collections.Generic;
using System.ServiceModel;
using Fhi.HelseId.Web.Hpr.Core;
using Fhi.HelseId.Web.Services;
using HprServiceReference;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fhi.HelseId.Web.Hpr
{
    //public interface IHprFactory
    //{
    //    IHPR2ServiceChannel? ServiceProxy { get; }

    //    [Obsolete("Use CreateHprService")]
    //    IHprService CreateHprRepository();
    //    IHprService CreateHprService();
    //}

    public interface IGodkjenteHprKategoriListe
    {
        IEnumerable<OId9060> Godkjenninger { get; }
    }

    /// <summary>
    /// Lag subklasse og sett opp for injection
    /// Legg til i denne listen de kategoriene som skal være godkjent
    /// Mulige verdier finnes i filen Kodekonstaner.g.cs
    /// Eks.: Verdi for Lege er:  Kodekonstanter.OId9060Lege
    /// </summary>
    public class GodkjenteHprKategoriListe : IGodkjenteHprKategoriListe
    {
        private readonly List<OId9060> godkjenninger = new();

        public void Add(OId9060 godkjent) => godkjenninger.Add(godkjent);
        public void AddRange(IEnumerable<OId9060> godkjente) => godkjenninger.AddRange(godkjente);
        public IEnumerable<OId9060> Godkjenninger => godkjenninger;
    }

    //public class HprFactory : IHprFactory
    //{
    //    private readonly IGodkjenteHprKategoriListe _godkjenninger;
    //    private readonly IMemoryCache _memoryCache;
    //    private readonly ILogger<HprFactory> _logger;
    //    private readonly ICurrentUser _currentUser;

    //    public IHPR2ServiceChannel? ServiceProxy { get; }

    //    public HprFactory(IOptions<HelseIdWebKonfigurasjon> hprKonfigurasjon, IGodkjenteHprKategoriListe godkjenninger, ICurrentUser currentUser, ILogger<HprFactory> logger)
    //    {
    //        _godkjenninger = godkjenninger;
    //        _logger = logger;
    //        _currentUser = currentUser;
    //        var config = hprKonfigurasjon.Value;
    //        _logger.LogDebug("Oppsett til HPR: {Url}", config.HprUrl);
    //        if (!config.UseHpr)
    //        {
    //            _logger.LogInformation("HprFactory: Hpr er avslått, se konfigurasjon");
    //            return;
    //        }
    //    }

    //    public IHprService CreateHprService() => new HprService(_currentUser, _logger).LeggTilGodkjenteHelsepersonellkategorier(_godkjenninger);

    //    [Obsolete("Use CreateHprService")]
    //    public IHprService CreateHprRepository() => CreateHprService();
    //}
}