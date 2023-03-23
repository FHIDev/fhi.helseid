using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Fhi.HelseId.Web.Handlers
{
    public class SecurityLevelClaimHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdWebKonfigurasjon configAuth;

        public SecurityLevelClaimHandler(IHelseIdWebKonfigurasjon configAuth)
        {
            this.configAuth = configAuth;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            var securityLevelClaim = (context.User.FindFirst(c => c.Type.ToLowerInvariant() == IdentityClaims.SecurityLevel));

            if (securityLevelClaim != null)
            {
                if (configAuth.SecurityLevels.Any(sl => sl.ToLowerInvariant() == securityLevelClaim.Value.ToLowerInvariant()))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
