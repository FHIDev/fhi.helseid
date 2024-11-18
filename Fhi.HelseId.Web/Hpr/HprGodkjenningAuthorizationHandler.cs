using System.Threading.Tasks;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Web.Hpr
{
    public class HprGodkjenningAuthorizationHandler : AuthorizationHandler<HprGodkjenningAuthorizationRequirement>
    {
        private readonly IHprFactory _hprFactory;
        private readonly IWhitelist _whitelist;

        private ILogger<HprGodkjenningAuthorizationHandler> Logger { get; }

        public HprGodkjenningAuthorizationHandler(IHprFactory hprFactory, IWhitelist whitelist, ILogger<HprGodkjenningAuthorizationHandler> logger)
        {
            _whitelist = whitelist;
            _hprFactory = hprFactory;
            Logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HprGodkjenningAuthorizationRequirement requirement)
        {
            var currentUser = context.User;
            var userlogName = currentUser.Name().ObfuscateName();
            Logger.LogTrace("HprGodkjenningAuthorizationHandler: Checking {Name}", userlogName);
            if (!currentUser.Identity?.IsAuthenticated ?? false)
            {
                Logger.LogWarning("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} er ikke autentisiert", userlogName);
                context.Fail();
                return;
            }
            var hprNummer = currentUser.HprNumber();
            if (hprNummer == null)
            {
                Logger.LogInformation("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} har ikke hprnummer.", userlogName);
                SjekkWhitelist();
                return;
            }
            var hprService = _hprFactory.CreateHprService();
            var erGodkjent = await hprService.SjekkGodkjenning(hprNummer);
            if (erGodkjent)
            {
                Logger.LogTrace("HprGodkjenningAuthorizationHandler: {Name} autentisert", userlogName);
                context.Succeed(requirement);
            }
            else
            {
                Logger.LogInformation("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} er ikke godkjent.", userlogName);
                SjekkWhitelist();
            }

            void SjekkWhitelist()
            {
                if (_whitelist.IsWhite(currentUser.PidPseudonym() ?? ""))
                {
                    Logger.LogInformation("HprGodkjenningAuthorizationHandler: Bruker {UserlogName} er whitelisted.", userlogName);
                    context.Succeed(requirement);
                    return;
                }
                context.Fail();
            }
        }
    }
}