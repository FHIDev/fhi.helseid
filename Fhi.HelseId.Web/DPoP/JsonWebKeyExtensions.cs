using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.DPoP;

public static class JsonWebKeyExtensions
{
    public static JsonWebKey AsDPoPJwkSecret(this JsonWebKey key)
    {
        var json = JsonSerializer.Serialize(key);
        var clonedKey = JsonSerializer.Deserialize<JsonWebKey>(json)!;
        clonedKey.Alg ??= "PS256";
        
        return clonedKey;
    }
}