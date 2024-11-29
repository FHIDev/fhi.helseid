using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.IdentityModel.JsonWebTokens;
using Fhi.HelseId.Integration.Tests.TestFramework.Extensions;

namespace Fhi.HelseId.Integration.Tests.TestFramework.AuthenticationScheme.TestAuthenticationScheme
{
    /// <summary>
    /// Authentication handler to simulate loggedin user
    /// </summary>
    internal class TestAuthenticationHandler : SignInAuthenticationHandler<TestAuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {

        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Options.AccessToken is not null)
            {
                var accessTokenJwt = new JsonWebToken(Options.AccessToken);
                var authProperties = new AuthenticationProperties();
                authProperties.Items["id_token"] = Options.IdToken;
                authProperties.Items["access_token"] = Options.AccessToken;
                authProperties.IsPersistent = true;
                
                var ticket = accessTokenJwt.CreateAuthenticationTicket(Scheme.Name, authProperties);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            if (Options.ClaimsIssuer is not null) 
            {
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(Options.UserClaims, "TestAuthentication")), Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }


            return Task.FromResult(AuthenticateResult.Fail(new Exception("")));
        }



        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }
    }

}
