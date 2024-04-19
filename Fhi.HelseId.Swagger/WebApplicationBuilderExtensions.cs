using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseId.Swagger;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddHelseIdTestTokens(this WebApplicationBuilder builder, SwaggerHelseIdConfiguration swaggerAuthConfig)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton(swaggerAuthConfig);
            builder.Services.AddSingleton<ITokenProxy, TokenProxy>();
        }

        return builder;
    }
}
