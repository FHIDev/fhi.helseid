using System.Net.Http.Headers;

namespace Fhi.TestFramework.Extensions
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
