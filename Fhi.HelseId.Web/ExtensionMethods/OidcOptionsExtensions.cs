using Fhi.HelseId.Web.DPoP;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Fhi.HelseId.Web.ExtensionMethods;

public static class OidcOptionsExtensions
{
    public static void EnableDPoP(this OpenIdConnectOptions options, bool requireDPoP)
    {
        options.Events.OnMessageReceived = context =>
        {
            var tokenHandler = context.HttpContext.RequestServices.GetRequiredService<IOidcDPoPTokenHandler>();
            tokenHandler.ValidateAuthorizationHeader(context, requireDPoP);

            return Task.CompletedTask;
        };

        options.Events.OnTokenValidated = async tokenValidatedContext =>
        {
            var tokenHandler = tokenValidatedContext.HttpContext.RequestServices.GetRequiredService<IOidcDPoPTokenHandler>();
            await tokenHandler.ValidateDPoPProof(tokenValidatedContext, requireDPoP);
        };
    }
}
