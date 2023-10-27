using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using System;
using IdentityModel.Client;

namespace Fhi.HelseId.Web.Infrastructure
{
    public static class CookieExtensions
    {
        public static bool UpdateAccessToken(this CookieValidatePrincipalContext ctx, string accessToken)
        {
            bool updated = ctx.Properties.UpdateTokenValue("access_token", accessToken);
            return updated;
        }

        public static bool UpdateRefreshToken(this CookieValidatePrincipalContext ctx, string refreshToken)
        {
            bool updated = ctx.Properties.UpdateTokenValue("refresh_token", refreshToken);
            return updated;
        }

        public static DateTime UpdateExpiresAt(this CookieValidatePrincipalContext ctx, int expiresAt)
        {
            var newExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(expiresAt);
            ctx.Properties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));
            return newExpiresAt;
        }

        public static DateTime UpdateTokens(this CookieValidatePrincipalContext ctx, TokenResponse tokenResponse)
        {
            ctx.UpdateAccessToken(tokenResponse.AccessToken);
            ctx.UpdateRefreshToken(tokenResponse.RefreshToken);
            var newExpiresAt = ctx.UpdateExpiresAt(tokenResponse.ExpiresIn);
            return newExpiresAt;
        }

    }
}
