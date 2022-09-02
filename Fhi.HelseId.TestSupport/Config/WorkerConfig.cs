using Fhi.HelseId.Worker;
using Microsoft.Extensions.Configuration;

namespace Fhi.HelseId.TestSupport.Config;

/// <summary>
/// This class resolves appsettings for HelseIdWorkerKonfigurasjon
/// </summary>
public class WorkerConfig : SetupBaseConfigTests
{
    public Fhi.HelseId.Worker.HelseIdWorkerKonfigurasjon WorkerKonfigurasjon { get; }

    public WorkerConfig(string configFile) : base(configFile, AppSettingsUsage.AppSettingsIsExplicit)
    {
        WorkerKonfigurasjon = Config.GetSection(nameof(Fhi.HelseId.Worker.HelseIdWorkerKonfigurasjon))
            .Get<Fhi.HelseId.Worker.HelseIdWorkerKonfigurasjon>();
    }

    protected override void Guard() { }
}

/// <summary>
/// This class resolves appsettings for HelseIdWorkerKonfigurasjon
/// The configFile is resolved explicitly
/// </summary>
public class ResolveHelseIdClientCredentialsConfig : SetupBaseConfigTests
{
    public HelseIdClientCredentialsConfiguration WorkerKonfigurasjon { get; }

    public ResolveHelseIdClientCredentialsConfig(string configFile) : base(configFile, AppSettingsUsage.AppSettingsIsExplicit)
    {
        WorkerKonfigurasjon = Config.GetSection(nameof(HelseIdClientCredentialsConfiguration))
            .Get<HelseIdClientCredentialsConfiguration>();
    }

    protected override void Guard() { }
}