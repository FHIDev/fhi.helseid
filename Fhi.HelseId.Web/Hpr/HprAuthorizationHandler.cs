using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    /// <summary>
    /// Handler for å sjekke HprNummer i HelseId. Slås på med UseHprNumber
    /// </summary>
    public class HprAuthorizationHandler : AuthorizationHandler<HprAuthorizationRequirement>
    {
        private readonly IWhitelist whitelist;

        private ILogger Logger { get; }
        public HprAuthorizationHandler(IWhitelist whitelist, ILogger<HprAuthorizationHandler> logger)
        {
            this.whitelist = whitelist;
            Logger = logger;
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HprAuthorizationRequirement requirement)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var currentUser = context.User;
            var userlogName = currentUser.Name().ObfuscateName();
            Logger.LogTrace("HprAuthorizationHandler: Checking {Name} with {PidPs}", userlogName,currentUser.PidPseudonym());
            if (currentUser.HprNumber()==null && !whitelist.IsWhite(currentUser?.PidPseudonym() ?? ""))
            {
                Logger.LogWarning("HprAuthorizationHandler: Failed. No HprNumber");
                context.Fail();
            }
            else
            {
                Logger.LogTrace("HprAuthorizationHandler: Succeeded");
                context.Succeed(requirement);
            }
        }
    }
}