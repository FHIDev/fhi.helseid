
using Fhi.HelseId.Common;

namespace Fhi.HelseId.Api
{
    public interface IHelseIdFeatures
    {
        bool UseHprNumber { get;  }
        bool UseHttps { get;  }
    }

    public class HelseIdApiKonfigurasjon : IAutentiseringkonfigurasjon, IHelseIdFeatures
    {
        public string Authority { get; set; } = "";

        public string ApiName { get; set; } = "";
        public string ApiScope { get; set; } = "";

        public bool AuthUse { get; set; } = true;
        public bool UseHprNumber { get; set; } = true;

        public bool UseHttps { get; set; } = true;
    }
}
