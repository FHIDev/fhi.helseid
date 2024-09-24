using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;

namespace Fhi.HelseId.Web.DPoP;

public class ProofKeyConfiguration
{
    public JsonWebKey ProofKey { get; }

    public ProofKeyConfiguration()
    {
        // Should we use the private key from the HelseID config instead?
        var rsaKey = new RsaSecurityKey(RSA.Create(2048));
        var jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);
        jsonWebKey.Alg = "PS256";
        var dpopJwk = JsonSerializer.Serialize(jsonWebKey);
        ProofKey = new JsonWebKey(dpopJwk);
    }
}
