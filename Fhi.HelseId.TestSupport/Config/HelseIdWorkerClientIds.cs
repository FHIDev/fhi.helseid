using Fhi.HelseId.Worker;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.TestSupport.Config
{
    public class HelseIdWorkerClientIds : SetupBaseConfigTests
    {
        public HelseIdWorkerKonfigurasjon HelseIdWorkerKonfigurasjonUnderTest { get; set; }
        public HelseIdWorkerClientIds(string configFile,AppSettingsUsage useOfAppsettings) : base(configFile, useOfAppsettings)
        {
            HelseIdWorkerKonfigurasjonUnderTest = Config.GetSection(nameof(HelseIdWorkerKonfigurasjon))
                .Get<HelseIdWorkerKonfigurasjon>();
        }

        protected override void Guard()
        {
            // Does nothing here
        }
    }
}