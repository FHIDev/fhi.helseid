using System.Linq;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Api.Services
{
        public interface ICurrentUser
        {
            string? Id { get; }
            string? HprNummer { get; }
            string? Name { get; }
            string? Pid { get; }
        }

        public class CurrentHttpUser : ICurrentUser
        {
            private readonly HttpContext httpContext;

            public CurrentHttpUser(IHttpContextAccessor httpContextAccessor)
            {
                httpContext = httpContextAccessor.HttpContext;

            }

            public string? Id => httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
            public string? HprNummer => httpContext.User.Claims.FirstOrDefault(x => x.Type == HprClaims.HprNummer)?.Value;
            public string? Name => httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Name)?.Value;
            public string? Pid => httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
    }
}
