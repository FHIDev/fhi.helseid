namespace Fhi.HelseId.Integration.Tests.TestFramework.NHNTTT.Dtos
{
    internal record DPoPProofParameters
    {
        public string? HtuClaimValue { get; set; }

        public string? HtmClaimValue { get; set; }

        public string? PrivateKeyForProofCreation { get; set; }

        public InvalidDPoPProofParameters? InvalidDPoPProofParameters { get; set; }

    }
}
