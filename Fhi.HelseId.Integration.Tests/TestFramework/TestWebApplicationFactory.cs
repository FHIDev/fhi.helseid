using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Integration.Tests.TestFramework
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private IConfiguration? _configuration { get; }
        private Action<IServiceCollection> _serviceCollection { get; }

        public TestWebApplicationFactory(IConfiguration configuration, Action<IServiceCollection> services)
        {
            _configuration = configuration;
            _serviceCollection = services;
        }

        public TestWebApplicationFactory(Action<IServiceCollection> services)
        {
            _serviceCollection = services;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (_configuration is not null)
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(_configuration);
                });
            }
            builder.ConfigureTestServices(_serviceCollection.Invoke);
        }
    }
}




