using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Integration.Tests.TestFramework
{
    /// <summary>
    /// Simplified builder for creating web applications using the new minimal hosting model.
    /// Used to test variations in your programs entry point (top-level statement),
    /// such as using different middleware or configurations.
    /// </summary>
    internal static class WebApplicationBuilderTestHost
    {
        public static WebApplicationBuilder CreateWebHostBuilder()
        {
            var builder = WebApplication.CreateBuilder([]);
            builder.WebHost.UseTestServer();

            return builder;
        }

        internal static WebApplicationBuilder WithConfiguration(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Configuration.AddConfiguration(configuration);
            return builder;
        }

        internal static WebApplicationBuilder WithServices(this WebApplicationBuilder builder, Action<IServiceCollection> services)
        {
            services.Invoke(builder.Services);
            return builder;
        }

        internal static WebApplication BuildApp(this WebApplicationBuilder builder, Action<WebApplication> appBuilder)
        {
            var app = builder.Build();
            appBuilder.Invoke(app);
            return app;
        }
    }
}
