using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Api.Handlers
{
    public class ApiScopeHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdApiKonfigurasjon _configAuth;
        private readonly ILogger<ApiScopeHandler> logger;

        public ApiScopeHandler(IHelseIdApiKonfigurasjon configAuth, ILogger<ApiScopeHandler> logger)
        {
            _configAuth = configAuth;
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            var scopeClaims = context.User.FindAll("scope");

            if (scopeClaims.Any(c => StringComparer.InvariantCultureIgnoreCase.Equals(c.Value, _configAuth.ApiScope)))
            {
                context.Succeed(requirement);
            }
            else
            {
                logger.LogError($"Fhi.HelseId.Api.Handlers.{nameof(ApiScopeHandler)}: Missing or invalid scope, access denied", scopeClaims);
            }

            return Task.CompletedTask;
        }
    }
}
