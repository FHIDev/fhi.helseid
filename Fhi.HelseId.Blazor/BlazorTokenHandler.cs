using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http.Headers;

namespace Fhi.HelseId.Blazor;

public class BlazorTokenHandler : DelegatingHandler
{
    public const string AnonymousOptionKey = "Anonymous";

    private BlazorTokenService tokenService;

    public BlazorTokenHandler(BlazorTokenService tokenService)
    {
        this.tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Options.All(x => x.Key != AnonymousOptionKey))
        {
            var accessToken = await tokenService.GetToken();
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
