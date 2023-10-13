using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace Fhi.HelseId.Web.Infrastructure
{
    public static class TokenExtensions
    {
        public static async  Task<string> AccessToken(this HttpContext ctx)
        {
            var ret =  await ctx.GetTokenAsync("access_token");
            if (ret == null)
                throw new NoTokenException("Missing access token");
            return ret;
        }

        public static async Task<string> IdentityToken(this HttpContext ctx)
        {
            var ret =  await ctx.GetTokenAsync("id_token");
            if (ret == null)
                throw new NoTokenException("Missing identity token");
            return ret;
        }

        public static async Task<string> RefreshToken(this HttpContext ctx)
        {
            var ret = await ctx.GetTokenAsync("refresh_token");
            if (ret == null)
                throw new NoTokenException("Missing refresh token");
            return ret;
        }
    }

    [Serializable]
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

        protected NoTokenException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
