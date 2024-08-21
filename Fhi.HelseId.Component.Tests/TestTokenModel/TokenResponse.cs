namespace Fhi.HelseId.Integration.Tests.TestTokenModel;

public class TokenResponse
{
    public bool IsError { get; set; }

    public SuccessResponse SuccessResponse { get; set; } = new();

    public ErrorResponse ErrorResponse { get; set; } = new();
}
