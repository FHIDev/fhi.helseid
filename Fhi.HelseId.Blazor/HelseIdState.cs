﻿using Microsoft.AspNetCore.Authentication;
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
            var correlationId = Guid.NewGuid().ToString();

            if (httpContext.Request.Headers.TryGetValue(CorrelationIdHandler.CorrelationIdHeaderName, out var values))
            {
                // if we find a correlation id on the request we update our default correlation Id
                correlationId = values.First() ?? correlationId;
            }
            else
            {
                // if we did not find a correlation Id, set it to the default one so other code that reads correlation Id can see it
                httpContext.Request.Headers.TryAdd(CorrelationIdHandler.CorrelationIdHeaderName, correlationId);
            }
            
            if (!httpContext.Response.Headers.TryGetValue(CorrelationIdHandler.CorrelationIdHeaderName, out _))
            {
                // if we did not find a correlation Id, set it to the default one so other code that reads correlation Id can see it
                httpContext.Response.Headers.TryAdd(CorrelationIdHandler.CorrelationIdHeaderName, correlationId);
            }

            return correlationId;
        }
    }
}
