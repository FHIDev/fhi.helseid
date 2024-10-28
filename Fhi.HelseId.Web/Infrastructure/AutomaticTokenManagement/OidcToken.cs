using System;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public class OidcToken
    {
        public OidcToken(string accessToken, string refreshToken, DateTimeOffset expiresOn, string json)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresOn = expiresOn;
            IsError = false;
            Error = null;
            ErrorDescription = "";
            Json = json;
        }
        public OidcToken(Exception ex, string errorDescription)
        {
            AccessToken = "";
            RefreshToken = "";
            ExpiresOn = DateTimeOffset.MinValue;
            IsError = true;
            Error = ex;
            ErrorDescription = errorDescription;
            Json = null;
        }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
        public bool IsError { get; set; }
        public Exception? Error { get; set; }
        public string ErrorDescription { get; set; }
        public string? Json { get; set; }
    }
}