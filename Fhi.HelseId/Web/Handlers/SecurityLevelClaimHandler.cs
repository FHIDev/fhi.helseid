using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Handlers
{
    public class SecurityLevelClaimHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdWebKonfigurasjon configAuth;
        private readonly ILogger<SecurityLevelClaimHandler> logger;

        public SecurityLevelClaimHandler(IHelseIdWebKonfigurasjon configAuth, ILogger<SecurityLevelClaimHandler> logger)
        {
            this.configAuth = configAuth;
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            logger.LogTrace("SecurityLevelClaimHandler: Validating");
            var securityLevelClaim = (context.User.FindFirst(c => c.Type.ToLowerInvariant() == IdentityClaims.SecurityLevel));

            if (securityLevelClaim != null)
            {
                if (configAuth.SecurityLevels.Any(sl => sl.ToLowerInvariant() == securityLevelClaim.Value.ToLowerInvariant()))
                {
                    logger.LogTrace("SecurityLevelClaimHandler: Succeeded");
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
