using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.DPoP;

public class ProofKeyConfiguration
{
    public JsonWebKey ProofKey { get; }

    public ProofKeyConfiguration(string jwkJson)
    {
        ProofKey = new JsonWebKey(jwkJson);
        ProofKey.Alg ??= "PS256";
    }

    public ProofKeyConfiguration(JsonWebKey key)
    {
        ProofKey = key;
    }
}
