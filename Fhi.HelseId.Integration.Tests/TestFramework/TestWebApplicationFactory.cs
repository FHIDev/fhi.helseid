using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private IConfiguration? _configuration { get; }
    private Action<IServiceCollection> _serviceCollection { get; }
    private Action<IApplicationBuilder>? _appBuilder { get; }

    public TestWebApplicationFactory(IConfiguration configuration, Action<IServiceCollection> services,
        Action<IApplicationBuilder>? appBuilder = null)
    {
        _configuration = configuration;
        _serviceCollection = services;
        _appBuilder = appBuilder;

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

        builder.Configure(app =>
        {
            _appBuilder?.Invoke(app);
        });

    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Add or replace services for testing
        });


        return base.CreateHost(builder);
    }
}