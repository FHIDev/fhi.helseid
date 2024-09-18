using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Integration.Tests.Setup.Programs.SingleScopeApi
{
    public class SingleScopeApi
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<SingleScopeApi>();
                });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var configAuth = new HelseIdApiKonfigurasjon
            {
                Authority = "https://helseid-sts.test.nhn.no/",
                ApiName = "fhi:helseid.testing.api",
                ApiScope = "fhi:helseid.testing.api/all",
                AuthUse = true,
                UseHttps = true,
                RequireContextIdentity = true,
            };
            services.AddHelseIdApiAuthentication(configAuth);
            services.AddHelseIdAuthorizationControllers(configAuth);
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory
        )
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/api/test", [Authorize] () => "Hello world!");
            });
            app.UseHttpsRedirection();
            app.UseAuthorization();
        }
    }
}
