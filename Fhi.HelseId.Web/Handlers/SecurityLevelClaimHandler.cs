using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
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

            if (!context.User.Identity?.IsAuthenticated ?? false)
            {
                logger.LogInformation("SecurityLevelClaimHandler: User is not authenticated, access denied");
                return Task.CompletedTask;
            }
            var securityLevel = "";
            if (context.User.Claims.Any(x => x.Type == IdentityClaims.SecurityLevel))
            {
                securityLevel = context.User.Claims.FirstOrDefault(x => x.Type.ToLowerInvariant() == IdentityClaims.SecurityLevel)?.Value;
            }
            else if (context.User.Claims.Any(x => x.Type.ToLowerInvariant() == IdentityClaims.SecurityLevelEnum))
            {
                var SecurityLevelEnum = context.User.Claims.FirstOrDefault(x => x.Type.ToLowerInvariant() == IdentityClaims.SecurityLevelEnum)?.Value;
                switch (SecurityLevelEnum)
                {
                    case "idporten-loa-substantial":
                        securityLevel = "3";
                        break;
                    case "idporten-loa-high":
                        securityLevel = "4";
                        break;
                }
            }
             
            if (securityLevel != null)
            {
                if (configAuth.SecurityLevels.Any(sl => string.Equals(sl, securityLevel, StringComparison.InvariantCultureIgnoreCase)))
                {
                    logger.LogTrace("SecurityLevelClaimHandler: Succeeded");
                    context.Succeed(requirement);
                }
                else
                {
                    logger.LogError("SecurityLevelClaimHandler: Invalid security level claim '{securityLevel}', access denied.", securityLevel);
                }
            }
            else
            {
                logger.LogError("SecurityLevelClaimHandler: No security level claim found, access denied");
            }

            return Task.CompletedTask;
        }
    }
}
