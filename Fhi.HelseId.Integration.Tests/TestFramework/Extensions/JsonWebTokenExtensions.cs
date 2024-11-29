using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fhi.HelseId.Integration.Tests.TestFramework.Extensions
{
    internal static class JsonWebTokenExtensions
    {
        internal static AuthenticationTicket CreateAuthenticationTicket(this JsonWebToken jwt, string scheme, AuthenticationProperties? properties = null)
        {
            var claimsIdentity = new ClaimsIdentity(jwt.Claims, "TestAuthentication");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), properties, scheme);
            return ticket;
        }
    }
}
