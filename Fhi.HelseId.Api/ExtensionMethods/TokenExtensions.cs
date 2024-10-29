using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fhi.HelseId.Api.ExtensionMethods;

public static class TokenExtensions
{
    public static async Task<string> AccessToken(this HttpContext ctx)
    {
        var ret = await ctx.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
        return ret ?? throw new NoTokenException("Missing access token");
    }

    public static async Task<string> IdentityToken(this HttpContext ctx)
    {
        var ret = await ctx.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
        return ret ?? throw new NoTokenException("Missing identity token");
    }

    public static async Task<string> RefreshToken(this HttpContext ctx)
    {
        var ret = await ctx.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
        return ret ?? throw new NoTokenException("Missing refresh token");
    }
}

public class NoTokenException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public NoTokenException()
    {
    }

    public NoTokenException(string message) : base(message)
    {
    }

    public NoTokenException(string message, Exception inner) : base(message, inner)
    {
    }
}