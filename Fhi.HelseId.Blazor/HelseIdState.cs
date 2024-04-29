using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Blazor
{
    public class HelseIdState : IScopedState
    {
        public bool HasBeenInitialized { get; set; }

        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTimeOffset TokenExpires { get; set; }
        public string CorrelationId { get; set; } = "";

        public async Task Populate(HttpContext context)
        {
            HasBeenInitialized = true;

            var tokenExpiry = await context.GetTokenAsync("expires_at");
            DateTimeOffset.TryParse(tokenExpiry, out var expiresAt);

            AccessToken = await context.GetTokenAsync("access_token") ?? "";
            RefreshToken = await context.GetTokenAsync("refresh_token") ?? "";
            TokenExpires = expiresAt;
            CorrelationId = GetCorrelationId(context);
        }

        private static string GetCorrelationId(HttpContext httpContext)
        {
            var header = string.Empty;

            if (httpContext.Request.Headers.TryGetValue(CorrelationIdHandler.CorrelationIdHeaderName, out var values))
            {
                header = values.FirstOrDefault();
            }
            else if (httpContext.Response.Headers.TryGetValue(CorrelationIdHandler.CorrelationIdHeaderName, out values))
            {
                header = values.FirstOrDefault();
            }

            return string.IsNullOrEmpty(header) ? Guid.NewGuid().ToString() : header;
        }
    }
}
