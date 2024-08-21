using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Component.Tests
{
    internal class JwtTokenGenerator
    {
        public static string GenerateToken(int expiresInMinutes)
        {
            using var rsa = RSA.Create(2048);
            var key = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            var claims = CreateClaims();
            var tokenDescriptor = CreateTokenDescriptor(expiresInMinutes, rsa, credentials, claims);
            var tokennn = new JwtSecurityToken();

            var publicKeyInfo = rsa.ExportSubjectPublicKeyInfo();
            var rsaPublicKey = Base64UrlEncode(publicKeyInfo);
            var sha1 = SHA1.Create();
            var x5t = Base64UrlEncode(sha1.ComputeHash(publicKeyInfo));

            var header = new JwtHeader(credentials)
            {
                { "kid", "78667F90DC11BF04BD9467D1F9120C4A4340B4CF" },
                { "x5c", new[] { rsaPublicKey } },
                { "x5t", x5t },
            };
            var payload = new JwtPayload
            {
                { JwtRegisteredClaimNames.Iss, "https://localhost:9001" },
                { JwtRegisteredClaimNames.Aud, "nhn:helseid-test-token-tjeneste" },
                {
                    JwtRegisteredClaimNames.Exp,
                    DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes).ToUnixTimeSeconds()
                },
                { JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            };
            foreach (var claim in claims)
            {
                payload.Add(claim.Type, claim.Value);
            }
            var tokennnn = new JwtSecurityToken(header, payload);
            var jwt = new JwtSecurityToken(header: header, payload: payload);
            var tokenHandler = new JwtSecurityTokenHandler();
            //var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenAsString = tokenHandler.WriteToken(jwt);
            return tokenAsString;
        }

        private static Claim[] CreateClaims() =>
            new[]
            {
                //  new Claim(JwtRegisteredClaimNames.Iss, "https://localhost:9001"),
                //  new Claim(JwtRegisteredClaimNames.Aud, "nhn:helseid-test-token-tjeneste"),
                new Claim("scope", "nhn:helseid-test-token-tjeneste/tokengenerering-med-clientid"),
                new Claim("client_id", "5c43298d-e3da-4fcc-8328-7b24ed821bdb"),
                new Claim("client_amr", "private_key_jwt"),
                new Claim("helseid://claims/client/amr", "rsa_private_key"),
                new Claim(
                    "nhn:helseid-test-token-tjeneste/client/claims/audience",
                    "fhi:helseid.testing.api"
                ),
                new Claim(JwtRegisteredClaimNames.Jti, "B8BB3F161D0F22839288AD65F40095D8"),
            };

        private static SecurityTokenDescriptor CreateTokenDescriptor(
            int expiresInMinutes,
            RSA rsa,
            SigningCredentials credentials,
            Claim[] claims
        )
        {
            var rsaParameters = rsa.ExportParameters(false);
            var jwk = new
            {
                kty = "RSA",
                e = Base64UrlEncode(rsaParameters.Exponent),
                n = Base64UrlEncode(rsaParameters.Modulus),
            };
            var jwtHeader = new JwtHeader(credentials)
            {
                { "kid", "78667F90DC11BF04BD9467D1F9120C4A4340B4CF" },
                { "x5c", new[] { Base64UrlEncode(rsa.ExportSubjectPublicKeyInfo()) } },
                {
                    "x5t",
                    Base64UrlEncode(SHA1.Create().ComputeHash(rsa.ExportSubjectPublicKeyInfo()))
                },
            };

            var publicKeyInfo = rsa.ExportSubjectPublicKeyInfo();
            var rsaPublicKey = Convert.ToBase64String(publicKeyInfo);
            var sha1 = SHA1.Create();
            var x5t = Convert.ToBase64String(sha1.ComputeHash(publicKeyInfo));

            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                Issuer = "https://localhost:9001",
                Audience = "nhn:helseid-test-token-tjeneste",
                SigningCredentials = credentials,
                // Header = JwtHeader,
                // AdditionalHeaderClaims = new Dictionary<string, object>
                // {
                //     { "kid", "78667F90DC11BF04BD9467D1F9120C4A4340B4CF" },
                //     { "x5c", new[] { Base64UrlEncode(rsa.ExportSubjectPublicKeyInfo()) } },
                //     {
                //         "x5t",
                //         Base64UrlEncode(SHA1.Create().ComputeHash(rsa.ExportSubjectPublicKeyInfo()))
                //     },
                // },
            };
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            base64 = base64.TrimEnd('=');
            base64 = base64.Replace('+', '-');
            base64 = base64.Replace('/', '_');
            return base64;
        }
    }
}
