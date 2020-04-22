using System;
using System.ServiceModel;
using HprServiceReference;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Fhi.HelseId.Web.Hpr
{
    public interface IHprFactory
    {
        IHPR2ServiceChannel? ServiceProxy { get; }
        IHprService CreateHprRepository();
    }

    public class HprFactory : IHprFactory
    {
        private ILogger<HprFactory> logger;
        public IHPR2ServiceChannel? ServiceProxy { get; }

        public HprFactory(IOptions<HprKonfigurasjon> hprKonfigurasjon, ILogger<HprFactory> logger)
        {
            this.logger = logger;
            var config = hprKonfigurasjon.Value;
            this.logger.LogDebug("Access til HPR: {Url}", config.Url);
            if (!config.UseHpr)
            {
                this.logger.LogInformation("HprFactory: Hpr er avslått, se konfigurasjon");
                return;
            }

            var userName = config.Brukernavn;
            var passord = config.Passord;
            var httpBinding = new WSHttpBinding(SecurityMode.Transport);
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var channelFactory = new ChannelFactory<IHPR2ServiceChannel>(httpBinding, new EndpointAddress(new Uri(config.Url)));
            channelFactory.Credentials.UserName.UserName = userName;
            channelFactory.Credentials.UserName.Password = passord;
            ServiceProxy = channelFactory.CreateChannel();
            ServiceProxy.Open();
        }

       

        public IHprService CreateHprRepository() => new HprService(this,logger);
    }
}
