using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace Fhi.HelseId.Web.Infrastructure
{
    public static class TokenExtensions
    {
        public static async  Task<string> AccessToken(this HttpContext ctx) => await ctx.GetTokenAsync("access_token");

        public static async Task<string> IdentityToken(this HttpContext ctx) => await ctx.GetTokenAsync("id_token");
        
    }
}
