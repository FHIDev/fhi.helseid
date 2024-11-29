using Fhi.HelseId.Integration.Tests.TestFramework.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Fhi.HelseId.Integration.Tests.TestFramework.CookieScheme
{
    internal class FakeCookieTicketDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        public string Protect(AuthenticationTicket data)
        {
            throw new NotImplementedException();
        }

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            throw new NotImplementedException();
        }

        public AuthenticationTicket? Unprotect(string? protectedText)
        {
               
            var jwt = new JsonWebToken(protectedText);
            AuthenticationTicket ticket = jwt.CreateAuthenticationTicket("scheme");

            return ticket;
        }

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
        {
            return Unprotect(protectedText);
        }
    }
}
