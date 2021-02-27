using System;

namespace Fhi.HelseId.Web
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
}