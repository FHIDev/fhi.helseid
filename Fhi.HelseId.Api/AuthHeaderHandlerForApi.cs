using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Exceptions;
using Fhi.HelseId.ExtensionMethods;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Api;

/// <summary>
/// This is to be used for Apis that need to send same access tokens onwards to further Apis
/// See https://github.com/reactiveui/refit#bearer-authentication
/// </summary>
public class AuthHeaderHandlerForApi : DelegatingHandler
{
    public const string AnonymousOptionKey = "Anonymous";

    private readonly IHttpContextAccessor contextAccessor;

    public AuthHeaderHandlerForApi(IHttpContextAccessor contextAccessor)
    {
        this.contextAccessor = contextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Options.Any(x => x.Key == AnonymousOptionKey))
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var ctx = contextAccessor.HttpContext ?? throw new NoContextException();
        var token = await ctx.AccessToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
