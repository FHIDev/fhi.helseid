using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Web.Services
{
    public interface ICurrentUser
    {
        string? Id { get; }
        string? Name { get; }
        string? HprNummer { get; }
        string? PidPseudonym { get; }
    }

    public class CurrentHttpUser: ICurrentUser
    {
        private readonly HttpContext httpContext;

        public CurrentHttpUser(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public string? Id => httpContext.User.Id();
        public string? Name => httpContext.User.Name();
        public string? HprNummer => httpContext.User.HprNumber();

        public string? PidPseudonym => httpContext.User.PidPseudonym();

    }
}
