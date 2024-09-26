using System.Linq;
using Fhi.HelseId.Common.DPoP;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Api.DPoP;

public static class DPoPExtensions
{
    private const string DPoPAuthorizationSchema = AuthorizationScheme.DPoP + " ";

    public static bool TryGetDPoPAccessToken(this HttpRequest request, out string token)
    {
        token = "";
        var authorization = request.Headers.Authorization.SingleOrDefault() ?? "";
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith(DPoPAuthorizationSchema))
        {
            return false;
        }
        token = authorization.Substring(DPoPAuthorizationSchema.Length);
        return true;
    }

    public static bool TryGetDPoPProof(this HttpRequest request, out string dPopProof)
    {
        dPopProof = request.Headers[DPoPHttpHeaders.ProofHeaderName].SingleOrDefault() ?? "";
        return !string.IsNullOrEmpty(dPopProof);
    }

    public static string CreateThumbprint(this JsonWebKey jwk)
    {
        return Base64UrlEncoder.Encode(jwk.ComputeJwkThumbprint());
    }
}