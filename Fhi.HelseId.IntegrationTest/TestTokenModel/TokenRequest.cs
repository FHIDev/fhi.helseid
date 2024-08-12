namespace Fhi.HelseId.Integration.Tests.TestTokenModel;

public class TokenRequest
{
    public ParametersGeneration GeneralClaimsParametersGeneration { get; set; }

    public ParametersGeneration UserClaimsParametersGeneration { get; set; }

    public GeneralParameters GeneralClaimsParameters { get; set; } = new();

    public UserClaimsParameters UserClaimsParameters { get; set; } = new();
}
