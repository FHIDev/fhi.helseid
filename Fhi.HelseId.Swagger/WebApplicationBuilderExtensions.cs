using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Swagger;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddHelseIdTestTokens(this WebApplicationBuilder builder, SwaggerHelseIdConfiguration swaggerAuthConfig, string[]? enabledEnvironments = null)
    {
        var currentEnvironmentName = builder.Environment.EnvironmentName;

        var isProduction = currentEnvironmentName.StartsWith("Prod", StringComparison.OrdinalIgnoreCase);

        if (enabledEnvironments?.Contains(currentEnvironmentName) == true && !isProduction)
        {
            builder.Services.AddSingleton(swaggerAuthConfig);
            builder.Services.AddSingleton<ITokenProxy, TokenProxy>();
        }

        return builder;
    }
}
