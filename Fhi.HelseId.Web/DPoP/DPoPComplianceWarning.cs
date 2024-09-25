using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Fhi.HelseId.Web.DPoP;

public class DPoPComplianceWarning(ILogger<DPoPComplianceWarning> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("This web application is not configured to require DPoP-tokens. " +
            $"This must be enabled in order to be compliant with the NHN Security profile. " +
            $"It can be enabled by setting the flag {nameof(HelseIdWebKonfigurasjon.RequireDPoPTokens)} " +
            $"to true in appsettings.json under 'HelseIdWebKonfigurasjon'.");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
