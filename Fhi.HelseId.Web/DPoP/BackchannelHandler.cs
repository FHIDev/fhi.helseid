using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Fhi.HelseId.Web.DPoP;

public class BackchannelHandler(
    IHttpContextAccessor httpContextAccessor,
    IDPoPTokenCreator tokenHelper)
    : DelegatingHandler
{
    private const string DPoPHeader = "DPoP";
    private const string DPoPNonceHeader = "DPoP-Nonce";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var requestHasJktContext = httpContext!.Items.ContainsKey(DPoPContext.ContextKey);
        
        if (requestHasJktContext)
        {
            SetDPoPHeader(request);
        }

        var response = await base.SendAsync(request, cancellationToken);
        var nonce = GetNonce(response.Headers);
           
        // If the STS returned a 400 bad request and provided a nonce, we can resend the request
        // with a DPoP header using the provided nonce from the STS.
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && !string.IsNullOrEmpty(nonce))
        {
            response.Dispose();
            SetDPoPHeader(request, nonce);

            return await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    private async void SetDPoPHeader(HttpRequestMessage request, string? nonceValue = null)
    {
        var proof = await tokenHelper.CreateSignedToken(
            method: request.Method,
            url: request.RequestUri!.Scheme + "://" + request.RequestUri!.Authority + request.RequestUri!.LocalPath,
            nonce: nonceValue);

        request.Headers.Remove(DPoPHeader);
        request.Headers.Add(DPoPHeader, proof);
    }

    private static string? GetNonce(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        return headers
            .FirstOrDefault(x => x.Key == DPoPNonceHeader)
            .Value?.FirstOrDefault();
    }
}