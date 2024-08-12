namespace Fhi.HelseId.Integration.Tests.TestTokenModel;

public class UserClaimsParameters
{
    /// helseid://claims/identity/pid
    public string Pid { get; set; } = string.Empty;

    /// helseid://claims/identity/pid_pseudonym
    public string PidPseudonym { get; set; } = string.Empty;

    /// helseid://claims/hpr/hpr_number
    public string HprNumber { get; set; } = string.Empty;

    /// name
    public string Name { get; set; } = string.Empty;

    /// given_name
    public string GivenName { get; set; } = string.Empty;

    /// middle_name
    public string MiddleName { get; set; } = string.Empty;

    /// family_name
    public string FamilyName { get; set; } = string.Empty;
}