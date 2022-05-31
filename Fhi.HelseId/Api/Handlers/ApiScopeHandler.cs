using System;
using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Microsoft.AspNetCore.Authorization;

namespace Fhi.HelseId.Api.Handlers
{
    public class ApiScopeHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdApiKonfigurasjon _configAuth;

        public ApiScopeHandler(IHelseIdApiKonfigurasjon configAuth)
        {
            _configAuth = configAuth;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            var scopeClaims = context.User.FindAll("scope");

            if(scopeClaims.Any(c => StringComparer.InvariantCultureIgnoreCase.Equals(c.Value, _configAuth.ApiScope)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
