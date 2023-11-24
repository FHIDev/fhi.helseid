using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Logs the ctor or member for a class, use for DI injected classes so you can see in the log the sequence of their creation
        /// For ctor, no parameters are neeed, for members, add classname as the first parameter
        /// A custom message can be added as a second parameter.  For a ctor, then just add empty string for classname
        /// </summary>
        public static void LogMember(this ILogger logger,string? className="",string? message="", [CallerMemberName] string? methodName=null)
        {
            logger.LogTrace("{class}.{method} {message}", className, methodName, message);
        }
    }
}
