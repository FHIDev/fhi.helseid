using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Api.Handlers
{
    /// <summary>
    /// This scope handler expects a single scope in the configuration, and should not be set if there are multiple scopes
    /// </summary>
    public class ApiSingleScopeHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdApiKonfigurasjon _configAuth;
        private readonly ILogger<ApiSingleScopeHandler> logger;

        public ApiSingleScopeHandler(IHelseIdApiKonfigurasjon configAuth, ILogger<ApiSingleScopeHandler> logger)
        {
            _configAuth = configAuth;
            this.logger = logger;
            logger.LogTrace("Fhi.HelseId.Api.Handlers.{class}: Enabled for {requirement}", nameof(ApiSingleScopeHandler),nameof(SecurityLevelOrApiRequirement));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            logger.LogTrace("ApiSingleScopeHandler: Validating");
            var scopeClaims = context.User.FindAll("scope").ToList();

            if (scopeClaims.Any(c => StringComparer.InvariantCultureIgnoreCase.Equals(c.Value, _configAuth.ApiScope)))
            {
                logger.LogTrace("ApiSingleScopeHandler: Succeeded");
                context.Succeed(requirement);
            }
            else
            {
                logger.LogError("Fhi.HelseId.Api.Handlers.{nameofApiMultiScopeHandler}: Missing or invalid scope {scopeClaims}, access denied", nameof(ApiMultiScopeHandler), string.Join(',', scopeClaims));
            }

            return Task.CompletedTask;
        }
    }
}
