﻿using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Fhi.HelseId.Common.Identity
{
    public static class ClientAssertion
    {
        public static string Generate(string clientId, string authority, SecurityKey securityKey, bool useIdPorten)
        {
            SigningCredentials signingCredentials;
            if (useIdPorten) {
                signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
            }
            else {
                signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha512);
            }
            var extraClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, clientId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };
             
            var payload = CreatePayload(clientId, authority, extraClaims, DateTime.UtcNow);
            var header = new JwtHeader(signingCredentials);
            UpdateJwtHeader(securityKey, header);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(new JwtSecurityToken(header, payload));
        }

        private static JwtPayload CreatePayload(string clientId, string audience, List<Claim>? claims = null, DateTime? issuedAt=null)
        {
            var payload = new JwtPayload(
               clientId,
               audience,
               null,
               DateTime.UtcNow,
               DateTime.UtcNow.AddSeconds(10), 
               issuedAt);

            if (claims == null)
            {
                return payload;
            }

            var jsonClaims = claims.Where(x => x.ValueType == "json");
            var normalClaims = claims.Except(jsonClaims);

            payload.AddClaims(normalClaims);

            return AddJsonToPayload(payload, jsonClaims);
        }

        // We need to handle json objects ourself as this isn't directly supported by JwtSecurityHandler yet
        private static JwtPayload AddJsonToPayload(JwtPayload payload, IEnumerable<Claim> jsonClaims)
        {
            var jsonTokens = jsonClaims.Select(x => new { x.Type, JsonValue = JToken.Parse(x.Value) }).ToArray();

            var jsonObjects = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Object).ToArray();
            var jsonObjectGroups = jsonObjects.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonObjectGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new IncompatibleClaimTypesException(string.Format("Can't add two claims where one is a JSON object and the other is not a JSON object ({0})", group.Key));
                }

                if (group.Skip(1).Any())
                {
                    // add as array
                    payload.Add(group.Key, group.Select(x => x.JsonValue).ToArray());
                }
                else
                {
                    // add just one
                    payload.Add(group.Key, group.First().JsonValue);
                }
            }

            var jsonArrays = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Array).ToArray();
            var jsonArrayGroups = jsonArrays.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonArrayGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new IncompatibleClaimTypesException(string.Format("Can't add two claims where one is a JSON array and the other is not a JSON array ({0})", group.Key));
                }

                var newArr = new List<JToken>();
                foreach (var arrays in group)
                {
                    var arr = (JArray)arrays.JsonValue;
                    newArr.AddRange(arr);
                }

                // add just one array for the group/key/claim type
                payload.Add(group.Key, newArr.ToArray());
            }

            var unsupportedJsonTokens = jsonTokens.Except(jsonObjects).Except(jsonArrays);
            var unsupportedJsonClaimTypes = unsupportedJsonTokens.Select(x => x.Type).Distinct();
            if (unsupportedJsonClaimTypes.Any())
            {
                var unsupportedClaimTypes = unsupportedJsonClaimTypes.Aggregate((x, y) => x + ", " + y);
                throw new UnsupportedJsonClaimTypeException($"Unsupported JSON type for claim types: {unsupportedClaimTypes}");
            }

            return payload;
        }

        private static void UpdateJwtHeader(SecurityKey key, JwtHeader header)
        {
            if (key is X509SecurityKey x509Key)
            {
                var thumbprint = Base64UrlEncoder.Encode(x509Key.Certificate.GetCertHash());
                var x5c = GenerateX5c(x509Key.Certificate);

                if (x509Key.PublicKey is not RSA pubKey)
                {
                    throw new UnsupportedSigningKeyTypeException("Only certificates based on RSA keys are supported at the moment");
                }

                var parameters = pubKey.ExportParameters(false);
                var exponent = Base64UrlEncoder.Encode(parameters.Exponent);
                var modulus = Base64UrlEncoder.Encode(parameters.Modulus);

                header.Add("x5c", x5c);
                header.Add("kty", pubKey.SignatureAlgorithm);
                header.Add("use", "sig");
                header.Add("x5t", thumbprint);
                header.Add("e", exponent);
                header.Add("n", modulus);
            }

            if (key is RsaSecurityKey rsaKey)
            {
                var parameters = rsaKey.Rsa?.ExportParameters(false) ?? rsaKey.Parameters;
                var exponent = Base64UrlEncoder.Encode(parameters.Exponent);
                var modulus = Base64UrlEncoder.Encode(parameters.Modulus);

                header.Add("kty", "RSA");
                header.Add("use", "sig");
                header.Add("e", exponent);
                header.Add("n", modulus);
            }
        }

        private static List<string> GenerateX5c(X509Certificate2 certificate)
        {

            var x5C = new List<string>();

            var chain = GetCertificateChain(certificate);
            if (chain != null)
            {
                foreach (var cert in chain.ChainElements)
                {
                    var x509Base64 = Convert.ToBase64String(cert.Certificate.RawData);
                    x5C.Add(x509Base64);
                }
            }
            return x5C;
        }

        private static X509Chain GetCertificateChain(X509Certificate2 cert)
        {
            var certificateChain = X509Chain.Create();
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            certificateChain.Build(cert);
            return certificateChain;
        }

        public class UnsupportedJsonClaimTypeException : Exception
        {
            public UnsupportedJsonClaimTypeException(string message) : base(message)
            {
            }
        }

        public class IncompatibleClaimTypesException : Exception
        {
            public IncompatibleClaimTypesException(string message) : base(message)
            {
            }
        }

        public class UnsupportedSigningKeyTypeException : Exception
        {
            public UnsupportedSigningKeyTypeException(string message) : base(message)
            {
            }
        }
    }
}
