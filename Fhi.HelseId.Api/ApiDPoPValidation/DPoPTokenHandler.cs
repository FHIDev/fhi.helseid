using HelseId.Samples.Common.ApiDPoPValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fhi.HelseId.Api.ApiDPoPValidation;

public interface IDPoPTokenHandler
{
    void ValidateAuthorizationHeader(MessageReceivedContext context);
    Task ValidateDPoPProof(TokenValidatedContext tokenValidatedContext);
}

public class DPoPTokenHandler(
    IHelseIdApiKonfigurasjon helseIdApiKonfigurasjon,
    IDPoPProofValidator dPoPProofValidator) : IDPoPTokenHandler
{
    public void ValidateAuthorizationHeader(MessageReceivedContext context)
    {
        var requestHasDPoPAccessToken = context.Request.TryGetDPoPAccessToken(out var dPopToken);

        if (requestHasDPoPAccessToken)
        {
            context.Token = dPopToken;
        }

        if (!requestHasDPoPAccessToken && helseIdApiKonfigurasjon.RequireDPoPTokens)
        {
            context.Fail("Request has no DPoP token, which is required");
        }
    }

    public async Task ValidateDPoPProof(TokenValidatedContext tokenValidatedContext)
    {
        var request = tokenValidatedContext.HttpContext.Request;

        // Get the access token:
        var requestIsDPoP = request.TryGetDPoPAccessToken(out var accessToken);

        if (!requestIsDPoP)
        {
            if (helseIdApiKonfigurasjon.RequireDPoPTokens)
            {
                tokenValidatedContext.Fail("Request has no DPoP token, which is required");
            }

            return;
        }

        if (!request.TryGetDPoPProof(out var dPopProof))
        {
            tokenValidatedContext.Fail("Missing DPoP proof");
            return;
        }

        // Get the cnf claim from the access token:
        var cnfClaimValue = tokenValidatedContext.Principal!.FindFirstValue(DPoPClaimNames.Confirmation);

        var data = new DPoPProofValidationData(request, dPopProof!, accessToken!, cnfClaimValue);

        var validationResult = await dPoPProofValidator.Validate(data);
        if (validationResult.IsError)
        {
            tokenValidatedContext.Fail(validationResult.ErrorDescription!);
        }
    }
}
