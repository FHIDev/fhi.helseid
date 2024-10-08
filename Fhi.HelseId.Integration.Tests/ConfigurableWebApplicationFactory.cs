using Fhi.HelseId.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Fhi.HelseId.Api.ExtensionMethods;

namespace Fhi.HelseId.Integration.Tests;

public class ConfigurableWebApplicationFactory<TProgram>(HelseIdApiKonfigurasjon helseIdApiKonfigurasjon) : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddHelseIdApiAuthentication(helseIdApiKonfigurasjon);
            services.AddHelseIdAuthorizationControllers(helseIdApiKonfigurasjon);
        });
    }
}
