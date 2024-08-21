namespace Fhi.HelseId.Integration.Tests.TestTokenModel;

public enum ParametersGeneration
{
    GenerateNone = 0,
    GenerateOnlyDefault = 1,
    GenerateOnlyFromNonEmptyParameterValues = 2,
    GenerateDefaultWithClaimsFromNonEmptyParameterValues = 3,
}