using System.Threading.Tasks;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    public class HprAuthorizationHandler : AuthorizationHandler<HprAuthorizationRequirement>
    {
        private readonly IWhitelist whitelist;

        private ILogger Logger { get; }
        public HprAuthorizationHandler(IWhitelist whitelist, ILogger<HprAuthorizationHandler> logger)
        {
            this.whitelist = whitelist;
            Logger = logger;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HprAuthorizationRequirement requirement)
        {
            var currentUser = context.User;
            var userlogName = currentUser.Name().ObfuscateName();
            Logger.LogTrace("HprAuthorizationHandler: Checking {Name} with {PidPs}", userlogName,currentUser.PidPseudonym());
            if (currentUser.HprNumber()==null && !whitelist.IsWhite(currentUser.PidPseudonym()))
            {
                Logger.LogWarning("HprAuthorizationHandler: Failed");
                context.Fail();
               
            } else
                context.Succeed(requirement);
            
        }
    }
}