namespace Fhi.HelseId.Web.DPoP;

internal struct DPoPClaimNames
{
    /// <summary>
    /// The value of the HTTP method (Section 9.1 of [RFC9110]) of the request to which the JWT is attached.
    /// https://datatracker.ietf.org/doc/html/rfc9449#section-4.2-4.4
    /// </summary>
    internal const string HttpMethod = "htm";

    /// <summary>
    /// The HTTP target URI (Section 7.1 of [RFC9110]) of the request to which the JWT is attached, without query and fragment parts.
    /// https://datatracker.ietf.org/doc/html/rfc9449#section-4.2-4.6
    /// </summary>
    internal const string HttpUrl = "htu";
}
