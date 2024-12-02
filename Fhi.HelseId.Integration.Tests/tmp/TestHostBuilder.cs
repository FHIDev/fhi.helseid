using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseId.Integration.Tests.TestFramework
{
    internal static class TestHostBuilder
    {
        internal static Action<IWebHostBuilder> Configure(IConfiguration configuration, Action<IServiceCollection> serviceCollection, Action<IApplicationBuilder> appBuilder)
        {
            Action<IWebHostBuilder> configure = builder =>
            {
                var webHostBuilder = builder.UseTestServer();
                webHostBuilder.AddServiceCollection(serviceCollection);
                webHostBuilder.AddApp(appBuilder);
                webHostBuilder.AddAppConfiguration(configuration);
            };

            return configure;
        }

        internal static Action<IWebHostBuilder> ConfigureWithStartup<T>(IConfiguration configuration, Action<IServiceCollection> serviceCollection)
            where T : class
        {
            Action<IWebHostBuilder> configure = builder =>
            {
                var webHostBuilder = builder.UseTestServer();
                webHostBuilder.UseStartup<T>();
                webHostBuilder.AddServiceCollection(serviceCollection);
                webHostBuilder.AddAppConfiguration(configuration);
            };

            return configure;
        }

    }

    internal static class IWebHostBuilderExtensions
    {
        internal static TestServer CreateTestServer(this Action<IWebHostBuilder> conf)
        {
            var builder = new HostBuilder().ConfigureWebHost(conf);
            var host = builder.Build();
            host.Start();

            return host.GetTestServer();
        }

        internal static IWebHostBuilder AddServiceCollection(this IWebHostBuilder builder, Action<IServiceCollection> serviceCollection)
        {
            return builder.ConfigureServices(serviceCollection.Invoke);
        }

        internal static IWebHostBuilder AddApp(this IWebHostBuilder builder, Action<IApplicationBuilder> appBuilder)
        {
            return builder.Configure(appBuilder.Invoke);
        }

        internal static IWebHostBuilder AddAppConfiguration(this IWebHostBuilder builder, IConfiguration configuration)
        {
            return builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddConfiguration(configuration);
            });
        }
    }





}
