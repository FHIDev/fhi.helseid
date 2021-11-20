using System;
using Fhi.HelseId.Common;

namespace Fhi.HelseId.Worker
{
    public class HelseIdApiKonfigurasjon
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Scope { get; set; } = "";

        public Uri Uri => new Uri(Url);
    }

    public class HelseIdWorkerKonfigurasjon : HelseIdClientKonfigurasjon
    {
        public HelseIdApiKonfigurasjon[] Apis { get; set; } = Array.Empty<HelseIdApiKonfigurasjon>();
    }


    public class HelseIdApiKonfigurasjonOutgoing
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
}