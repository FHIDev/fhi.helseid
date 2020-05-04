using System.Threading.Tasks;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    public class LegeAuthorizationHandler : AuthorizationHandler<LegeAuthorizationRequirement>
    {
        private readonly IHprFactory _hprFactory;
        private readonly IWhitelist whitelist;

        private ILogger<LegeAuthorizationHandler> Logger { get; }
        public LegeAuthorizationHandler(IHprFactory hprFactory, IWhitelist whitelist, ILogger<LegeAuthorizationHandler> logger)
        {
            this.whitelist = whitelist;
            _hprFactory = hprFactory;
            Logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, LegeAuthorizationRequirement requirement)
        {
            var currentUser = context.User;
            var userlogName = currentUser.Name().ObfuscateName();
            Logger.LogTrace("LegeAuthorizationHandler: Checking {Name}", userlogName);
            if (!currentUser.Identity.IsAuthenticated)
            {
                Logger.LogWarning("LegeAuthorizationHandler: Bruker {UserlogName} er ikke autentisiert", userlogName);
                context.Fail();
                return;
            }
            var hprNummer = currentUser.HprNumber();
            if (hprNummer == null)
            {
                Logger.LogWarning("LegeAuthorizationHandler: Bruker {UserlogName} har ikke hprnummer. Info: {@User}", userlogName, context.User);
                SjekkWhitelist();
                return;
            }
            var hprRepository = _hprFactory.CreateHprRepository();
            var erLege = await hprRepository.SjekkGodkjenning(hprNummer);
            if (erLege)
            {
                Logger.LogTrace("LegeAuthorizationHandler: {Name} autentisert som lege", userlogName);
                context.Succeed(requirement);
            }
            else
            {
                Logger.LogWarning("LegeAuthorizationHandler: Bruker {UserlogName} er ikke lege. Info: {@User}", userlogName, context.User);
                SjekkWhitelist();
            }

            void SjekkWhitelist()
            {
                if (whitelist.IsWhite(currentUser.PidPseudonym()))
                {
                    Logger.LogWarning("LegeAuthorizationHandler: Bruker {UserlogName} er whitelisted. Info: {@User}", userlogName,
                        context.User);
                    context.Succeed(requirement);
                    return;
                }
                context.Fail();
            }
        }
    }


}
