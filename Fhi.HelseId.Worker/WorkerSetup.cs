using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Worker
{
    public class WorkerSetup
    {

        public IConfiguration Config { get; }
        private HelseIdWorkerKonfigurasjon HelseIdConfig { get; }

        public WorkerSetup(IConfiguration config)
        {
            Config = config;
            HelseIdConfig = Config.GetWorkerKonfigurasjon();

        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (HelseIdConfig.AuthUse)
            {
                services.AddAccessTokenManagement(options =>
                {
                    foreach (var api in HelseIdConfig.Apis)
                    {
                        options.Client.Clients.Add(api.Name, new IdentityModel.Client.ClientCredentialsTokenRequest
                        {
                            Address = HelseIdConfig.Authority,
                            ClientId = HelseIdConfig.ClientId,
                            ClientSecret = HelseIdConfig.ClientSecret,

                            Scope = api.Scope
                        });
                    }
                });
            }


            foreach (var api in HelseIdConfig.Apis)
            {
                if (HelseIdConfig.AuthUse)
                    ConfigureService(services, api);
                else
                    ConfigureServiceNoAuth(services, api);
            }


        }

        public void ConfigureService(IServiceCollection services, HelseIdApiKonfigurasjon api)
        {
            services.AddClientAccessTokenClient(api.Name, api.Name, configureClient: client =>
            {
                client.BaseAddress = new Uri(api.Url.TrimEnd('/'));

            });
        }

        public void ConfigureServiceNoAuth(IServiceCollection services, HelseIdApiKonfigurasjon api)
        {
            services.AddHttpClient(api.Name, client =>
            {
                client.BaseAddress = new Uri(api.Url.TrimEnd('/'));
            });
        }
    }
}
