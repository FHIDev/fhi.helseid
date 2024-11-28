using System;
using System.Threading.Tasks;
using Fhi.HelseId.Common.ExtensionMethods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Common.Middleware
{
    public class RedirectOnExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string redirectPage;
        private ILogger Logger { get; }

        public RedirectOnExceptionMiddleware(RequestDelegate next, string redirectPage, ILogger<RedirectOnExceptionMiddleware> logger)
        {
            logger.LogMember();
            Logger = logger;
            this.next = next;
            this.redirectPage = redirectPage;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Handling exception by redirecting to {page}", redirectPage);
                context.Response.Redirect(redirectPage);
            }
        }
    }

    public static class RedirectOnExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseRedirectOnException(
            this IApplicationBuilder builder,
            string redirectPage)
        {
            return builder.UseMiddleware<RedirectOnExceptionMiddleware>(redirectPage);
        }
    }
}