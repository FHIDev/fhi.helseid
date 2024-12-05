using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Use if you have an existing entrypoint (top-level statements in Program.cs) file that you want to test
/// </summary>
public class WebApplicationFactoryTestHost<T> : WebApplicationFactory<T> where T : class
{
    private IConfiguration? _configuration { get; }
    private Action<IServiceCollection> _serviceCollection { get; }
    private Action<IApplicationBuilder>? _appBuilder { get; }

    public WebApplicationFactoryTestHost(IConfiguration configuration, Action<IServiceCollection> services,
        Action<IApplicationBuilder>? appBuilder = null)
    {
        _configuration = configuration;
        _serviceCollection = services;
        _appBuilder = appBuilder;
    }

    public WebApplicationFactoryTestHost(Action<IServiceCollection> services)
    {
        _serviceCollection = services;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseStartup<T>();
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
}