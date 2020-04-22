using System.Threading.Tasks;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    public class LegeAuthorizationHandler : AuthorizationHandler<LegeAuthorizationRequirement>
    {
        private readonly IHprFactory _hprFactory;

        private ILogger<LegeAuthorizationHandler> Logger { get; }
        public LegeAuthorizationHandler(IHprFactory hprFactory,ILogger<LegeAuthorizationHandler> logger)
        {
            _hprFactory = hprFactory;
            Logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, LegeAuthorizationRequirement requirement)
        {
            var userlogName = context.User.Name().ObfuscateName();
            Logger.LogDebug("LegeAuthorizationHandler: Checking {Name}", userlogName);
            if (!context.User.Identity.IsAuthenticated)
            {
                Logger.LogWarning("LegeAuthorizationHandler: Bruker {UserlogName} er ikke autentisiert", userlogName);
                context.Fail();
                return;
            }
            var hprNummer = context.User.HprNumber();
            if (hprNummer == null)
            {
                Logger.LogWarning("LegeAuthorizationHandler: Bruker {UserlogName} har ikke hprnummer. Info: {@User}", userlogName,context.User);
                context.Fail();
                return;
            }
            var hprRepository = _hprFactory.CreateHprRepository();
            var erLege = await hprRepository.SjekkGodkjenning(hprNummer);
            if (erLege)
            {
                Logger.LogDebug("LegeAuthorizationHandler: {Name} autentisert som lege", userlogName);
                context.Succeed(requirement);
            }
            else
            {
                Logger.LogWarning("LegeAuthorizationHandler: Bruker {UserlogName} er ikke lege. Info: {@User}", userlogName, context.User);
                context.Fail();
            }
        }
    }


}
