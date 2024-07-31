using Fhi.HelseId.Common;
using Microsoft.AspNetCore.Authorization;

namespace Fhi.HelseId.Web.Handlers
{
    public class NoAuthorizationHandler : AuthorizationHandler<NoAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NoAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
