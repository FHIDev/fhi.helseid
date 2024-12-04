using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Integration.Tests.TestFramework
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private IConfiguration? Configuration { get; }
        private Action<IServiceCollection> ServiceCollection { get; }

        public TestWebApplicationFactory(IConfiguration configuration, Action<IServiceCollection> services)
        {
            Configuration = configuration;
            ServiceCollection = services;
        }

        public TestWebApplicationFactory(Action<IServiceCollection> services)
        {
            ServiceCollection = services;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (Configuration is not null)
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(Configuration);
                });
            }

            builder.ConfigureTestServices(ServiceCollection.Invoke);
        }
    }
}
