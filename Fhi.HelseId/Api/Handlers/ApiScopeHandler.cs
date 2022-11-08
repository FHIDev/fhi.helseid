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
        private readonly ILogger<ApiScopeHandler> _logger;

        public ApiScopeHandler(IHelseIdApiKonfigurasjon configAuth, ILogger<ApiScopeHandler> logger)
        {
            _configAuth = configAuth;
            _logger = logger;
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
                string scopeHandler = nameof(ApiScopeHandler);
                _logger.LogError("Fhi.HelseId.Api.Handlers.{scopeHandler}: Missing or invalid scope ({scopeClaims}), access denied", scopeClaims,scopeHandler);
            }

            return Task.CompletedTask;
        }
    }
}
