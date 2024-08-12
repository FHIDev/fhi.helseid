namespace Fhi.HelseId.Integration.Tests.TestTokenModel;

public class GeneralParameters
{
    /// scope
    public IList<string> Scope { get; set; } = new List<string>();

    /// client_id
    public string ClientId { get; set; } = string.Empty;

    /// helseid://claims/client/claims/orgnr_parent
    public string OrgnrParent { get; set; } = string.Empty;

    /// helseid://claims/client/claims/orgnr_child
    public string OrgnrChild { get; set; } = string.Empty;
}