using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;



namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement
{
    public class AutomaticTokenManagementCookieEvents : CookieAuthenticationEvents
    {
        private readonly TokenEndpointService _service;
        private readonly AutomaticTokenManagementOptions _options;
        private readonly ILogger _logger;
        private readonly ISystemClock _clock;
        private readonly IHelseIdWebKonfigurasjon config;
        private readonly IRefreshTokenReporter refreshTokenReporter;

        private static readonly ConcurrentDictionary<string, bool> PendingRefreshTokenRequests = new();

        public AutomaticTokenManagementCookieEvents(
            TokenEndpointService service,
            IOptions<AutomaticTokenManagementOptions> options,
            ILogger<AutomaticTokenManagementCookieEvents> logger,
            ISystemClock clock,IHelseIdWebKonfigurasjon config,
            IRefreshTokenReporter refreshTokenReporter)
        {
             _service = service;
            _options = options.Value;
            _logger = logger;
            _clock = clock;
            this.config = config;
            this.refreshTokenReporter = refreshTokenReporter;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            _logger.LogTrace("{class}:{method} - Starting checking token and possible refresh", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
            await _options.CookieEvents.ValidatePrincipal(context);

            var tokens = context.Properties.GetTokens()?.ToList();
            if (tokens == null || !tokens.Any())
            {
                _logger.LogTrace("No tokens found in cookie properties. SaveTokens must be enabled for automatic token refresh.");
                return;
            }

            var refreshToken = tokens.SingleOrDefault(t => t.Name == OpenIdConnectParameterNames.RefreshToken);
            if (refreshToken == null)
            {
                _logger.LogTrace("No refresh token found in cookie properties. A refresh token must be requested and SaveTokens must be enabled.");
                return;
            }

            var expiresAt = tokens.SingleOrDefault(t => t.Name == "expires_at");
            if (expiresAt == null)
            {
                _logger.LogTrace("No expires_at value found in cookie properties.");
                return;
            }
            refreshTokenReporter.AddIfNotExist(refreshToken.Value, "", "", "ValidatePrincipal");
            var dtExpires = DateTimeOffset.Parse(expiresAt.Value, CultureInfo.InvariantCulture);
            var dtRefresh = dtExpires.Subtract(_options.RefreshBeforeExpiration); //.Subtract(new TimeSpan(0,7,0)); // For testing it faster
            _logger.LogTrace(
                $"ValidatePrincipal: expires_at: {dtExpires}, refresh_before: {_options.RefreshBeforeExpiration}, refresh_at: {dtRefresh}, now: {_clock.UtcNow}, refresh_token: {refreshToken.Value}");
            if (config.UseApis)
            {
                if (dtRefresh < _clock.UtcNow)
                {
                    var shouldRefresh = PendingRefreshTokenRequests.TryAdd(refreshToken.Value, true);
                    _logger.LogTrace("{class}:{method} - Should refresh: {shouldRefresh}, No Of tokens in PendingRefreshTokenRequests {count} ", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal),shouldRefresh,PendingRefreshTokenRequests.Count);
                    if (shouldRefresh)
                    {
                        try
                        {
                            var response = await _service.RefreshTokenAsync(refreshToken.Value);

                            if (response.IsError)
                            {
                                _logger.LogTrace("Error refreshing token: {error}, {errordesc}\n{json}", response.Error, response.ErrorDescription, response.Json);
                                refreshTokenReporter.Dump();
                                context.RejectPrincipal();
                                return;
                            }
                            refreshTokenReporter.Add(refreshToken.Value,response.RefreshToken,response.AccessToken, "ValidatePrincipal-l.91");
                            context.Properties.UpdateTokenValue("access_token", response.AccessToken);
                            context.Properties.UpdateTokenValue("refresh_token", response.RefreshToken);
                            _logger.LogTrace("{class}.{method} - Refresh tokens: Current {current}, New {new}", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal),refreshToken.Value,response.RefreshToken);
                            var newExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);
                            context.Properties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));
                            _logger.LogTrace($"SignInAsync now as it expires at: {newExpiresAt}");
                            context.ShouldRenew = true;
                            await context.HttpContext.SignInAsync(context.Principal, context.Properties);
                        }
                        finally
                        {
                            PendingRefreshTokenRequests.TryRemove(refreshToken.Value, out _);
                        }
                    }
                    else
                    {
                        _logger.LogTrace("{class}:{method} -: Refresh token already requested, value {refreshtoken}", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal),refreshToken.Value);
                    }
                }
                else
                {
                    _logger.LogTrace("{class}:{method} -: No need to refresh token yet.", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
                }
            }
            else
            {
                _logger.LogTrace("{class}:{method} - No Apis in configuration",nameof(AutomaticTokenManagementCookieEvents),nameof(ValidatePrincipal));
            }

            _logger.LogTrace("{class}:{method} finished", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
        }

        public override async Task SigningOut(CookieSigningOutContext context)
        {
            _logger.LogTrace("{class}:{method}: SigningOut", nameof(AutomaticTokenManagementCookieEvents), nameof(SigningOut));
            await _options.CookieEvents.SigningOut(context);

            if (_options.RevokeRefreshTokenOnSignout == false) return;

            var result = await context.HttpContext.AuthenticateAsync();

            if (!result.Succeeded)
            {
                _logger.LogTrace("Can't find cookie for default scheme. Might have been deleted already.");
                return;
            }

            var tokens = result.Properties.GetTokens()?.ToList();
            if (tokens == null || !tokens.Any())
            {
                _logger.LogTrace("No tokens found in cookie properties. SaveTokens must be enabled for automatic token revocation.");
                return;
            }

            var refreshToken = tokens.SingleOrDefault(t => t.Name == OpenIdConnectParameterNames.RefreshToken);
            if (refreshToken == null)
            {
                _logger.LogTrace("No refresh token found in cookie properties. A refresh token must be requested and SaveTokens must be enabled.");
                return;
            }

            var response = await _service.RevokeTokenAsync(refreshToken.Value);

            if (response.IsError)
            {
                _logger.LogTrace("Error revoking token: {error}", response.Error);
                return;
            }
        }

        public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            return _options.CookieEvents.RedirectToAccessDenied(context);
        }

        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            return _options.CookieEvents.RedirectToLogin(context);
        }

        public override Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context)
        {
            return _options.CookieEvents.RedirectToLogout(context);
        }

        public override Task RedirectToReturnUrl(RedirectContext<CookieAuthenticationOptions> context)
        {
            return _options.CookieEvents.RedirectToReturnUrl(context);
        }

        public override Task SignedIn(CookieSignedInContext context)
        {
            _logger.LogTrace("{class}:{method}", nameof(AutomaticTokenManagementCookieEvents), nameof(SignedIn));
            return _options.CookieEvents.SignedIn(context);
        }

        public override Task SigningIn(CookieSigningInContext context)
        {
            _logger.LogTrace("{class}:{method}", nameof(AutomaticTokenManagementCookieEvents), nameof(SigningIn));
            return _options.CookieEvents.SigningIn(context);
        }
    }
}
