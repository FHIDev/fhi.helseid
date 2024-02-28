using Fhi.HelseId.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Fhi.HelseId.Refit;

public static class Extensions
{
    public static HelseidRefitBuilder AddHelseidRefitBuilder(this WebApplicationBuilder builder, string? configSection = null, RefitSettings? refitSettings = null)
    {
        var config = builder.Configuration
            .GetSection(configSection ?? nameof(HelseIdWebKonfigurasjon))
            .Get<HelseIdWebKonfigurasjon?>() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));

        return new HelseidRefitBuilder(builder.Services, config, refitSettings);
    }

    public static HelseidRefitBuilder AddHelseidRefitBuilder(this IServiceCollection services, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings = null)
    {
        return new HelseidRefitBuilder(services, config, refitSettings);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static T UseCorrelationId<T>(this T app) where T : IApplicationBuilder
    {
        var options = app.ApplicationServices.GetService<HelseidRefitBuilderOptions>();
        if (options == null)
        {
            throw new Exception("You need to call builder.AddHelseidRefitBuilder() before using app.UseCorrelationId()");
        }

        if (options.UseCorrelationId)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseHeaderPropagation();
        }

        return app;
    }
}