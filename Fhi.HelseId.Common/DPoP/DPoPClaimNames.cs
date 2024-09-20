namespace Fhi.HelseId.Common.DPoP;

public struct DPoPClaimNames
{
    /// <summary>
    /// DPoP access token hash
    /// </summary>
    public const string DPoPAccessTokenHash = "ath";

    /// <summary>
    /// DPoP HTTP method
    /// </summary>
    public const string DPoPHttpMethod = "htm";

    /// <summary>
    /// DPoP HTTP URL
    /// </summary>
    public const string DPoPHttpUrl = "htu";

    /// <summary>
    /// The confirmation
    /// </summary>
    public const string Confirmation = "cnf";

    /// <summary>
    /// Values for strongly typed JWTs
    /// </summary>
    public struct JwtTypes
    {
        /// <summary>
        /// OAuth 2.0 access token
        /// </summary>
        public const string AccessToken = "at+jwt";

        /// <summary>
        /// DPoP proof token
        /// </summary>
        public const string DPoPProofToken = "dpop+jwt";
    }

    /// <summary>
    /// Values for the cnf claim
    /// </summary>
    public struct ConfirmationMethods
    {
        /// <summary>
        /// JSON web key thumbprint
        /// </summary>
        public const string JwkThumbprint = "jkt";
    }
}
