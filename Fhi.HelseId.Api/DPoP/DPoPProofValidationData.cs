using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Api.DPoP;

public class DPoPProofValidationData(HttpRequest request, string proofToken, string accessToken, string? cnfClaimValueFromAccessToken)
{
    public string Url { get; set; } = request.Scheme + "://" + request.Host + request.PathBase + request.Path;

    public string HttpMethod { get; set; } = request.Method;

    public string ProofToken { get; set; } = proofToken;

    public string AccessTokenHash { get; set; } = Common.DPoP.AccessTokenHash.Sha256(accessToken);

    public string? CnfClaimValueFromAccessToken { get; set; } = cnfClaimValueFromAccessToken;

    public JsonWebKey? JsonWebKey { get; set; }

    public IDictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();

    public string? TokenId { get; set; }

    public string? JktClaimValueFromAccessToken { get; set; }
}