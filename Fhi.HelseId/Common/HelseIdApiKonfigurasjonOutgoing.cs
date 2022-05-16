using System;

namespace Fhi.HelseId.Common
{
    [Obsolete("Use for now until replaced")]
    public class HelseIdApiKonfigurasjonOutgoing : HelseIdCommonKonfigurasjon
    {
        public string Name { get; set; } = "";
        public string ApiUrl { get; set; } = "";
        public string Scope { get; set; } = "";
        public string ProxyUrl { get; set; } = "";
        public bool UseProxy { get; set; } = false;
        public Uri ApiUri => BaseAddressUtil.ToUri(ApiUrl);
        public Uri? ProxyUri => UseProxy ? new Uri(ProxyUrl) : null;

        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
    }

    /// <summary>
    /// Use this for an outgoing API configuration. 
    /// </summary>
    public class HelseIdApiOutgoingKonfigurasjon : HelseIdCommonKonfigurasjon
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";

        public string Scope { get; set; } = "";
        public Uri Uri => new Uri(Url);
    }

    /// <summary>
    /// This is the schema for the appsetting for outgoing APIs.
    /// </summary>
    public class HelseIdApiOutgoingKonfigurasjoner
    {
        public HelseIdApiOutgoingKonfigurasjon[] Apis { get; set; } = Array.Empty<HelseIdApiOutgoingKonfigurasjon>();
    }

}