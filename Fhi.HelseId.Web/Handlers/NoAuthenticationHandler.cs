using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Fhi.HelseId.Web.Handlers
{
    public class NoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly NoAuthenticationUser _noAuthenticationUserConfig;


#if NET6_0
        public NoAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IOptions<HelseIdWebKonfigurasjon> helseIdWebOptions)
            : base(options, logger, encoder, clock)
        {
            _noAuthenticationUserConfig = helseIdWebOptions.Value.NoAuthenticationUser ?? throw new ArgumentException(nameof(HelseIdWebKonfigurasjon.NoAuthenticationUser));
        }
#else
        public NoAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IOptions<HelseIdWebKonfigurasjon> helseIdWebOptions)
            : base(options, logger, encoder)
        {
            _noAuthenticationUserConfig = helseIdWebOptions.Value.NoAuthenticationUser ?? throw new ArgumentException(nameof(HelseIdWebKonfigurasjon.NoAuthenticationUser));
        }
#endif

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = _noAuthenticationUserConfig.Claims.Select(c => new Claim(IdentityClaims.Prefix + c.Key, c.Value));
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class NoAuthenticationUser
    {
        public Dictionary<string, string> Claims { get; set; } = new();
    }
}