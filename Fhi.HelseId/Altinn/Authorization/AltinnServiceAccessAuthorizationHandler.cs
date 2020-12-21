using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Fhi.HelseId.Altinn.Authorization
{
    /// <summary>
    /// Handler for Altinn service authorization requirements. The handler verifies against Altinn that the user has
    /// been delegated access to the service given in the requirement for an organization given by the application-local
    /// organization claim.
    /// </summary>
    /// <see cref="IAltinnServiceOwnerClient"/>
    /// <see cref="LocalClaims.AppLocalOrganization"/>
    public class AltinnServiceAuthorizationHandler : AuthorizationHandler<AltinnServiceAuthorizationRequirement>
    {
        private readonly IAltinnServiceOwnerClient serviceOwnerClient;

        public AltinnServiceAuthorizationHandler(IAltinnServiceOwnerClient serviceOwnerClient)
        {
            this.serviceOwnerClient = serviceOwnerClient;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AltinnServiceAuthorizationRequirement requirement)
        {
            var subject = context.User.Pid();
            if (subject == null)
            {
                context.Fail();
            }

            var reportee = context.User.Organization();
            if (reportee == null)
            {
                context.Fail();
            }

            var hasDelegation = await serviceOwnerClient.HasDelegation(subject, reportee, requirement.AltinnServiceCode, requirement.AltinnServiceEditionCode);
            if (!hasDelegation)
            {
                context.Fail();
            }

            context.Succeed(requirement);
        }
    }
}
