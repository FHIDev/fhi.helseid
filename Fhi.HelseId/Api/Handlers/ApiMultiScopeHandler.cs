using System.Linq;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Api.Handlers
{
    /// <summary>
    /// At least one scope in the list of access token scopes must be present in the list of allowed scopes
    /// </summary>
    public class ApiMultiScopeHandler : AuthorizationHandler<SecurityLevelOrApiRequirement>
    {
        private readonly IHelseIdApiKonfigurasjon _configAuth;
        private readonly ILogger<ApiMultiScopeHandler> logger;

        public ApiMultiScopeHandler(IHelseIdApiKonfigurasjon configAuth, ILogger<ApiMultiScopeHandler> logger)
        {
            _configAuth = configAuth;
            this.logger = logger;
            logger.LogTrace("Fhi.HelseId.Api.Handlers.{class}: Enabled for {requirement}", nameof(ApiMultiScopeHandler), nameof(SecurityLevelOrApiRequirement));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SecurityLevelOrApiRequirement requirement)
        {
            var clientId = context.User.FindFirst("client_id")?.Value??"???";
            var clientName = context.User.FindFirst("helseid://claims/client/client_name")?.Value??"???";
            logger.LogInformation("ApiMultiScopeHandler: Validating, Request ClientId {clientId} ClientName {clientName}",clientId,clientName);
            var scopeClaims = context.User.FindAll("scope").Where(s => s.Value.StartsWith(_configAuth.ApiName)).ToList();
            foreach (var claim in scopeClaims)
            {
                logger.LogInformation($"Fhi.HelseId.Api.Handlers.{nameof(ApiMultiScopeHandler)}: Scope claim: {claim.Value}");
            }
            if (!scopeClaims.Any())
            {
                logger.LogError($"Fhi.HelseId.Api.Handlers.{nameof(ApiMultiScopeHandler)}: No scopes found, access denied");
                return Task.CompletedTask;
            }
            var scopes = scopeClaims.Select(o=>o.Value.Trim().ToLower());
            var allowedScopes = _configAuth.ApiScope.Split(",").Select(o=>o.Trim().ToLower()).ToList();
            if (!allowedScopes.Any())
            {
                logger.LogError("No scopes defined in configuration");
                return Task.CompletedTask;
            }
            foreach (var allowedScope in allowedScopes)
            {
                logger.LogInformation("Fhi.HelseId.Api.Handlers.{class}: Allowed scope: {allowedScope}", nameof(ApiMultiScopeHandler), allowedScope);
            }
            var matches = scopes.Intersect(allowedScopes);
            if (matches.Any())
            {
                logger.LogTrace("ApiMultiScopeHandler: Succeeded");
                context.Succeed(requirement);
            }
            else
            {
                logger.LogError("Fhi.HelseId.Api.Handlers.{nameofApiMultiScopeHandler}: Missing or invalid scope {scopeClaims}, access denied", nameof(ApiMultiScopeHandler),string.Join(',',scopeClaims));
            }

            return Task.CompletedTask;
        }
    }



}
