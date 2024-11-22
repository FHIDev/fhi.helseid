using System.Net.Http.Headers;

namespace Fhi.HelseId.Integration.Tests.Extensions
{
    internal static class HttpClientExtensions
    {
        internal static HttpClient AddBearerAuthorizationHeader(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token
            );

            return client;
        }
    }
}
