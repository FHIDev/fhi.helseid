using Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Integration.Tests.TestFramework.CookieScheme
{
   
    internal class FakeCookieManager : ICookieManager
    {
        public void AppendResponseCookie(HttpContext context, string key, string? value, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public void DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public string GetRequestCookie(HttpContext context, string key)
        {
            var task = TTTTokenService.GetHelseIdToken(TTTTokenRequests.DefaultToken("fhi-api-access"));

            return task.Result;

        }
    }

}
