using System;
using System.Threading.Tasks;
using Fhi.HelseId.ExtensionMethods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Common.Middleware
{
    public class UnhandledExceptionLoggerMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger Logger { get; }
        
        public UnhandledExceptionLoggerMiddleware(RequestDelegate next, ILogger<UnhandledExceptionLoggerMiddleware> logger)
        {
            logger.LogMember();
            Logger = logger;
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                //SerilogUserInfoEnricher.EnrichWithUserInfo(context);
                Logger.LogError(ex, "Unhandled exception");
                throw;
            }
        }
    }

    public static class UnhandledExceptionLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseUnhandledExceptionLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnhandledExceptionLoggerMiddleware>();
        }
    }
}