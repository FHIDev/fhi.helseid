using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Net.Mime;

namespace Fhi.HelseId.Swagger;

public static class WebApplicationExtensions
{
    public static RouteHandlerBuilder UseHelseIdTestTokenEndpoint(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            return app.MapPost(".swagger-dev/token", async Task<IResult> ([FromForm] IFormCollection form, ITokenProxy tokenProxy) =>
            {
                var requestParameters = form.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.First());
                var jsonResult = await tokenProxy.RequestToken(requestParameters);

                return Results.Text(jsonResult, MediaTypeNames.Application.Json);
            })
            .DisableAntiforgery()
            .ExcludeFromDescription();
        }

        return new RouteHandlerBuilder([]);
    }
}
