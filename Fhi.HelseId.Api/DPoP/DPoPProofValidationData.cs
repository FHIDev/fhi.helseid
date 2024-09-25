using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Api.DPoP;

public class DPoPProofValidationData
{
    public DPoPProofValidationData(HttpRequest request, string proofToken, string accessToken, string? cnfClaimValueFromAccessToken)
    {
        Url = request.Scheme + "://" + request.Host + request.PathBase + request.Path;
        HttpMethod = request.Method;
        ProofToken = proofToken;
        AccessTokenHash = HashAccessToken(accessToken);
        CnfClaimValueFromAccessToken = cnfClaimValueFromAccessToken;
    }

    private static string HashAccessToken(string accessToken)
    {
        using var sha = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(accessToken);
        var hash = sha.ComputeHash(bytes);

        return Base64UrlEncoder.Encode(hash);
    }

    public string Url { get; set; }

    public string HttpMethod { get; set; }

    public string ProofToken { get; set; }

    public string AccessTokenHash { get; set; }

    public string? CnfClaimValueFromAccessToken { get; set; }

    public JsonWebKey? JsonWebKey { get; set; }

    public IDictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();

    public string? TokenId { get; set; }

    public string? JktClaimValueFromAccessToken { get; set; }
}