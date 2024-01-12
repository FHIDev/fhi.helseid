using System.Web;

namespace Fhi.HelseId.Blazor;

public class FhiHeaderDelegationHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = request
            .Headers
            .ToDictionary(x => x.Key, x => x.Value.First());

        foreach (var header in headers)
        {
            if (header.Key.StartsWith("fhi-"))
            {
                request.Headers.Remove(header.Key);
                request.Headers.Add(header.Key, HttpUtility.HtmlEncode(header.Value));
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}

