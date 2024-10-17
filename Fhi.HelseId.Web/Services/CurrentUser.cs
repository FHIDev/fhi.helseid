using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.Logging;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Common.ExtensionMethods;
using Fhi.HelseId.Common.Identity;

namespace Fhi.HelseId.Web.Services;

/// <summary>
/// For Web backend
/// </summary>
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

/// <summary>
/// For Web backend
/// </summary>
public class CurrentHttpUser : ICurrentUser
{
    public CurrentHttpUser(IHttpContextAccessor httpContextAccessor, ILogger<CurrentHttpUser> logger)
    {
        logger.LogMember(); 
        var httpContext = httpContextAccessor.HttpContext ?? throw new NoHttpContextException($"{nameof(CurrentHttpUser)}.ctor : No HttpContext found. This has to be called when there is a request");
        Id = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
        HprNummer = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimsPrincipalExtensions.HprNummer)?.Value;
        Name = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Name)?.Value;
        Pid = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Pid)?.Value;
        PidPseudonym = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.PidPseudonym)?.Value;
        if(httpContext.User.Claims.Any(x => x.Type == IdentityClaims.SecurityLevel))
        {
            SecurityLevel = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.SecurityLevel)?.Value;
        }
        else if (httpContext.User.Claims.Any(x => x.Type == IdentityClaims.SecurityLevelEnum))
        {
            var SecurityLevelEnum = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.SecurityLevelEnum)?.Value;
            switch (SecurityLevelEnum)
            {
                case "idporten-loa-substantial":
                    SecurityLevel = "Level3";
                    break;
                case "idporten-loa-high":
                    SecurityLevel = "Level4";
                    break;
            }
        }
        AssuranceLevel = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.AssuranceLevel)?.Value;
        Network = httpContext.User.Claims.FirstOrDefault(x => x.Type == IdentityClaims.Network)?.Value;
    }

    public string? Id { get; }
    public string? Name { get; }
    public string? HprNummer { get; }
    public string? PidPseudonym { get; }
    public string? Pid { get; }
    public string? SecurityLevel { get; }
    public string? AssuranceLevel { get; }
    public string? Network { get; }

}

public class NoHttpContextException : Exception
{
    public NoHttpContextException()
    {
    }

    public NoHttpContextException(string message) : base(message)
    {
    }

    public NoHttpContextException(string message, Exception inner) : base(message, inner)
    {
    }
}