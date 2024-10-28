using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Fhi.HelseId.Common.Configuration;
using Fhi.HelseId.Web.Handlers;

namespace Fhi.HelseId.Web;

public interface IHelseIdHprFeatures
{
    bool UseHprNumber { get; }
    bool UseHprPolicy { get; }
    string HprUsername { get; set; }
    string HprPassword { get; set; }
    string HprUrl { get; set; }
}

public interface IHelseIdWebKonfigurasjon : IHelseIdHprFeatures, IHelseIdClientKonfigurasjon
{
    bool UseIdPorten { get; set; }
    string[] SecurityLevels { get; }
    bool UseProtectedPaths { get; set; }
    RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; set; }
    ApiOutgoingKonfigurasjon[] Apis { get; set; }
    bool UseApis { get; }
    bool UseRefreshTokenStore { get; }
    NoAuthenticationUser NoAuthenticationUser { get; }

    /// <summary>
    /// Enables DPoP support in the authorizaiton code flow.
    /// </summary>
    public bool UseDPoPTokens { get; }
}


[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
public class HelseIdWebKonfigurasjon : HelseIdClientKonfigurasjon, IHelseIdWebKonfigurasjon
{
    public string DevelopmentRedirectUri { get; set; } = "/";

    public string[] SecurityLevels { get; set; } = [];

    public override IEnumerable<string> BaseScopes { get; set; } = [
                    "openid",
                    "profile",
                    "helseid://scopes/identity/pid",
                    "helseid://scopes/identity/pid_pseudonym",
                    "helseid://scopes/identity/security_level"
                ];


    private bool useHprNumber = false;
    public bool UseHprNumber
    { 
        get { return useHprNumber; } 
        set {
            useHprNumber = value;
            var hprString = "helseid://scopes/hpr/hpr_number";
            if (value && !HprScope.Contains(hprString))
            {
                HprScope.Append(hprString);
            } 
        } 
    } 
    public bool UseHprPolicy { get; set; } = false;

    public string HprUsername { get; set; } = "";
    public string HprPassword { get; set; } = "";

    public string HprUrl { get; set; } = "";

    public bool UseProtectedPaths { get; set; } = true;

    public bool UseRefreshTokenStore { get; set; } = false;

    public bool UseApis => Apis.Any();

    public RedirectPagesKonfigurasjon RedirectPagesKonfigurasjon { get; set; } = new();

    public int Validate()
    {
        throw new NotImplementedException();
    }

    public ApiOutgoingKonfigurasjon[] Apis { get; set; } = Array.Empty<ApiOutgoingKonfigurasjon>();

    public NoAuthenticationUser NoAuthenticationUser { get; set; } = new();

    public bool UseDPoPTokens { get; set; }
    public bool UseIdPorten { get; set; } = true;

    public Uri UriToApiByName(string name)
    {
        var url = Apis.FirstOrDefault(o => o.Name == name)?.Url ?? throw new InvalidApiNameException(name);
        return new Uri(url);
    }
}