using System;
using System.Diagnostics.CodeAnalysis;

namespace Fhi.HelseId.Web
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public class HelseIdWebKonfigurasjon
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
