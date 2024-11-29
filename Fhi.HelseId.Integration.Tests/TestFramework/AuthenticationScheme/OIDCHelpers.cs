
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Integration.Tests.TestFramework.AuthenticationScheme
{
    class TestStateDataFormat : ISecureDataFormat<AuthenticationProperties>
    {
        private AuthenticationProperties Data { get; set; } = new AuthenticationProperties();

        public string Protect(AuthenticationProperties data)
        {
            return "protected_state";
        }

        public string Protect(AuthenticationProperties data, string? purpose)
        {
            throw new NotImplementedException();
        }

        public AuthenticationProperties Unprotect(string? protectedText)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string?>()
                {
                    { ".xsrf", "correlationId" },
                    { OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, "redirect_uri" },
                    { "testkey", "testvalue" }
                });
            properties.RedirectUri = "http://testhost/redirect";
            return properties;
        }

        public AuthenticationProperties Unprotect(string? protectedText, string? purpose)
        {
            throw new NotImplementedException();
        }
    }



    class TestTokenHandler : TokenHandler
    {
        public override Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
        {
            var jwt = new JwtSecurityToken();
            return Task.FromResult(new TokenValidationResult()
            {
                SecurityToken = new JsonWebToken(jwt.EncodedHeader + "." + jwt.EncodedPayload + "."),
                ClaimsIdentity = new ClaimsIdentity("customAuthType"),
                IsValid = true
            });
        }

        public override SecurityToken ReadToken(string token)
        {
            return new JsonWebToken(token);
        }
    }



    class TestBackchannel : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.Equals("/tokens", request.RequestUri?.AbsolutePath, StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage()
                {
                    Content =
                   new StringContent("{ \"id_token\": \"my_id_token\", \"access_token\": \"my_access_token\" }", Encoding.ASCII, "application/json")
                });
            }
            if (string.Equals("/user", request.RequestUri?.AbsolutePath, StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage() { Content = new StringContent("{ }", Encoding.ASCII, "application/json") });
            }

            throw new NotImplementedException(request.RequestUri?.ToString());
        }
    }
}
