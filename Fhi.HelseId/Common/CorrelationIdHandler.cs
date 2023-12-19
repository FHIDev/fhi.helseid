using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Fhi.HelseId;

public class CorrelationIdHandler : DelegatingHandler
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdHandler()
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correalationId = Guid.NewGuid().ToString();

        if (!request.Headers.TryGetValues(CorrelationIdHeaderName, out var values))
        {
            correalationId = values!.First();
        }
        else
        {
            request.Headers.Add(CorrelationIdHeaderName, correalationId);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (!response.Headers.TryGetValues(CorrelationIdHeaderName, out _))
        {
            response.Headers.Add(CorrelationIdHeaderName, correalationId);
        }

        return response;
    }
}
