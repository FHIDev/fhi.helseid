using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Fhi.HelseId.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Refit;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public static class WebApplicationExtensions
    {
        public static HelseidRefitBuilderForBlazor AddHelseIdForBlazor(this WebApplicationBuilder builder, string? configSection = null, RefitSettings? refitSettings = null)
        {
            var config = builder.Configuration
                .GetSection(configSection ?? nameof(HelseIdWebKonfigurasjon))
                .Get<HelseIdWebKonfigurasjon?>() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon)); ;

            return new HelseidRefitBuilderForBlazor(builder.Services, config, refitSettings);
        }

        public static HelseidRefitBuilderForBlazor AddHelseIdForBlazor(this IServiceCollection services, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings = null)
        {
            return new HelseidRefitBuilderForBlazor(services, config, refitSettings);
        }

        /// <summary>
        /// Add a scoped state that will be populated using a HttpContext. This allows you to easily get values from the anywhere.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static StateHandlerBuilder AddStateHandlers(this WebApplicationBuilder builder)
        {
            return AddStateHandlers(builder.Services);
        }

        /// <summary>
        /// Add a scoped state that will be populated using a HttpContext. This allows you to easily get values from the anywhere.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static StateHandlerBuilder AddStateHandlers(this IServiceCollection services)
        {
            var existingOptionsService = services.FirstOrDefault(x => x.ServiceType == typeof(StateHandlerOptions));
            var options = existingOptionsService?.ImplementationInstance as StateHandlerOptions;
            return new StateHandlerBuilder(services, options ?? new StateHandlerOptions());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static WebApplication UseHelseIdForBlazor(this WebApplication app)
        {
            var options = app.Services.GetService<HelseidRefitBuilderForBlazorConfig>();
            if (options == null)
            {
                throw new Exception("You need to call builder.AddHelseIdForBlazor() before using app.UseHelseIdForBlazor()");
            }

            app.UseMiddleware<BlazortContextMiddleware>();

            if (options.UseCorrelationId)
            {
                app.UseMiddleware<CorrelationIdMiddleware>();
                app.UseHeaderPropagation();
            }

            if (options.UseLogoutUrl)
            {
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path.Equals(options.LogOutUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        await context.SignOutAsync(HelseIdContext.Scheme, new AuthenticationProperties
                        {
                            RedirectUri = options.LoggedOutRedirectUrl,
                        });

                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties
                        {
                            RedirectUri = options.LoggedOutRedirectUrl,
                        });

                        return;
                    }

                    await next();
                });
            }

            return app;
        }
    }
}
