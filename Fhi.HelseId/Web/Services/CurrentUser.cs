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
        string? Pid { get; }
        string? SecurityLevel { get; }
        string? AssuranceLevel { get; }
        string? Network { get; }
    }

    public class CurrentHttpUser: ICurrentUser
    {
        private readonly HttpContext? httpContext;

        public CurrentHttpUser(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public string? Id => httpContext?.User.Id();
        public string? Name => httpContext?.User.Name();
        public string? HprNummer => httpContext?.User.HprNumber();

        public string? PidPseudonym => httpContext?.User.PidPseudonym();
        public string? Pid => httpContext?.User.Pid();

        public string? SecurityLevel => httpContext?.User.SecurityLevel();
        public string? AssuranceLevel => httpContext?.User.AssuranceLevel();
        public string? Network => httpContext?.User.Network();

    }
}
