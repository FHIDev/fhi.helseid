using System;
using System.Diagnostics.CodeAnalysis;
using Fhi.HelseId.Common;

namespace Fhi.HelseId.Web
{
    public interface IHelseIdHprFeatures
    {
        bool UseHprNumber { get; }
    }

    public interface IHelseIdWebKonfigurasjon : IAutentiseringkonfigurasjon, IHelseIdHprFeatures
    {
        bool UseHttps { get; }
        string Authority { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string[] Scopes { get; }
        string AcrValues { get; }
        bool Debug { get; }
    }


    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public class HelseIdWebKonfigurasjon : IHelseIdHprFeatures, IHelseIdWebKonfigurasjon
    {
        public bool AuthUse { get; set; } = true;

        public bool UseHttps { get; set; } = true;

        public bool UseHprNumber { get; set; } = true;
        public string Authority { get; set; } = "";

        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";
        public string[] Scopes { get; set; } = Array.Empty<string>();

        public string AcrValues { get; set; } = "";

        public bool Debug { get; set; } = false;
    }
}
