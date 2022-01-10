using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fhi.HelseId.Api;
using Fhi.HelseId.Common;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Fhi.HelseId.Web.Handlers
{
    public class SecurityLevelClaimHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdWebKonfigurasjon _configAuth;

        public SecurityLevelClaimHandler(IHelseIdWebKonfigurasjon configAuth)
        {
            _configAuth = configAuth;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            var securityLevelClaim = (context.User.FindFirst(c => c.Type.ToLowerInvariant() == IdentityClaims.SecurityLevel));

            if (securityLevelClaim != null)
            {
                if (_configAuth.SecurityLevels.Any(sl => sl.ToLowerInvariant() == securityLevelClaim.Value.ToLowerInvariant()))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
