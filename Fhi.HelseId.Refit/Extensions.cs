using Fhi.HelseId.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Refit;

namespace Fhi.HelseId.Refit;

public static class Extensions
{
    public static HelseidRefitBuilder AddHelseidRefitBuilder(this WebApplicationBuilder builder, string? configSection = null, RefitSettings? refitSettings = null)
    {
        var config = builder.Configuration
            .GetSection(configSection ?? nameof(HelseIdWebKonfigurasjon))
            .Get<HelseIdWebKonfigurasjon?>() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon)); ;

        return new HelseidRefitBuilder(builder, config, refitSettings);
    }

    public static HelseidRefitBuilder AddHelseidRefitBuilder(this WebApplicationBuilder builder, RefitSettings? refitSettings = null)
    {
        var config = builder.Configuration
            .GetSection(nameof(HelseIdWebKonfigurasjon))
            .Get<HelseIdWebKonfigurasjon?>() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon)); ;

        return new HelseidRefitBuilder(builder, config, refitSettings);
    }
}