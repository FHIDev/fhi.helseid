using System;
using System.Collections.Generic;
using System.ServiceModel;
using Fhi.HelseId.Web.Hpr.Core;
using HprServiceReference;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Fhi.HelseId.Web.Hpr
{
    public interface IHprFactory
    {
        IHPR2ServiceChannel? ServiceProxy { get; }
        
        [Obsolete("Use CreateHprService")]
        IHprService CreateHprRepository();
        IHprService CreateHprService();
    }

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

    public class HprFactory : IHprFactory
    {
        private readonly IGodkjenteHprKategoriListe godkjenninger;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<HprFactory> logger;
        public IHPR2ServiceChannel? ServiceProxy { get; }

        public HprFactory(IOptions<HelseIdWebKonfigurasjon> hprKonfigurasjon, IGodkjenteHprKategoriListe godkjenninger, IMemoryCache memoryCache, ILogger<HprFactory> logger)
        {
            this.godkjenninger = godkjenninger;
            this.memoryCache = memoryCache;
            this.logger = logger;
            var config = hprKonfigurasjon.Value;
            this.logger.LogDebug("Oppsett til HPR: {Url}", config.HprUrl);
            if (!config.UseHprNumber)
            {
                this.logger.LogInformation("HprFactory: Hpr er avslått, se konfigurasjon");
                return;
            }
            
            var userName = config.HprUsername;
            var passord = config.HprPassword;
            var httpBinding = new WSHttpBinding(SecurityMode.Transport);
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var channelFactory = new ChannelFactory<IHPR2ServiceChannel>(httpBinding, new EndpointAddress(new Uri(config.HprUrl)));
            channelFactory.Credentials.UserName.UserName = userName;
            channelFactory.Credentials.UserName.Password = passord;
            ServiceProxy = channelFactory.CreateChannel();
            ServiceProxy.Open();
        }


        public IHprService CreateHprService() => new HprService(this, memoryCache, logger).LeggTilGodkjenteHelsepersonellkategorier(godkjenninger);

        [Obsolete("Use CreateHprService")]
        public IHprService CreateHprRepository() => CreateHprService();
    }
}
