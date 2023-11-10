using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Common.Identity;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Api.Services;

/// <summary>
/// For Api only 
/// </summary>
public interface ICurrentUser
{
    string? Id { get; }
    string? HprNummer { get; }
    string? Name { get; }
    string? Pid { get; }
    public string? PidPseudonym { get; }
    public IEnumerable<string> Scopes { get; }
}

/// <summary>
/// For Api access only
/// </summary>
public class CurrentHttpUser : ICurrentUser
{
    public CurrentHttpUser(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        Id = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
        HprNummer = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimsPrincipalExtensions.HprNummer)
            ?.Value;
        Name = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Name)?.Value;
        Pid = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
        PidPseudonym = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.PidPseudonym)?.Value;
        Scopes = httpContext.User.FindAll("scope").Select(o => o.Value).ToList();
    }

    public string? Id { get; }
    public string? HprNummer { get; }
    public string? Name { get; }
    public string? Pid { get; }
    public string? PidPseudonym { get; }
    public IEnumerable<string> Scopes { get; }

}