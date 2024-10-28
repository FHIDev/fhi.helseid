using System;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public record OidcToken
    {
        public OidcToken(string accessToken, string refreshToken, DateTimeOffset expiresAt, string json)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            IsError = false;
            Error = null;
            ErrorDescription = "";
            Json = json;
        }
        public OidcToken(Exception ex, string errorDescription)
        {
            AccessToken = "";
            RefreshToken = "";
            ExpiresAt = DateTimeOffset.MinValue;
            IsError = true;
            Error = ex;
            ErrorDescription = errorDescription;
            Json = null;
        }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsError { get; set; }
        public Exception? Error { get; set; }
        public string ErrorDescription { get; set; }
        public string? Json { get; set; }
    }
}