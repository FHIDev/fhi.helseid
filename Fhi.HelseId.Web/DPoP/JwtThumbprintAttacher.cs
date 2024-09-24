using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.HelseId.Web.DPoP;

public interface IProofRedirector
{
    void AttachThumbprint(RedirectContext ctx);
}

public class JwtThumbprintAttacher(ProofKeyConfiguration keyConfiguration) : IProofRedirector
{
    public void AttachThumbprint(RedirectContext ctx)
    {
        var jkt = Base64UrlEncoder.Encode(keyConfiguration.ProofKey.ComputeJwkThumbprint());

        ctx.Properties.Items[DPoPContext.ContextKey] = "true";
        ctx.ProtocolMessage.Parameters["dpop_jkt"] = jkt;
    }
}
