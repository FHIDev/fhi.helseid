﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fhi.HelseId.Common.ExtensionMethods;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
public class AutomaticTokenManagementCookieEvents : CookieAuthenticationEvents
{
    private readonly TokenEndpointService _service;
    private readonly AutomaticTokenManagementOptions _tokenConfig;
    private readonly ILogger _logger;
    private readonly ISystemClock _clock;
    private readonly HelseIdWebKonfigurasjon _helseIdConfig;
    private readonly IRefreshTokenStore _refreshTokenStore;

    private static readonly ConcurrentDictionary<string, bool> PendingRefreshTokenRequests = new();

    public AutomaticTokenManagementCookieEvents(
        TokenEndpointService service,
        IOptions<AutomaticTokenManagementOptions> tokenOptions,
        ILogger<AutomaticTokenManagementCookieEvents> logger,
        ISystemClock clock,
        IOptions<HelseIdWebKonfigurasjon> helseIdOptions,
        IRefreshTokenStore refreshTokenStore)
    {
        logger.LogMember();
        _service = service;
        _tokenConfig = tokenOptions.Value;
        _logger = logger;
        _clock = clock;
        _helseIdConfig = helseIdOptions.Value;
        _refreshTokenStore = refreshTokenStore;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context.Principal == null)
        {
            throw new InvalidOperationException("Property 'context.Principal' cannot be null.");
        }

        var user = new UserByIdentity((ClaimsIdentity?)context.Principal.Identity);
        _logger.LogTrace("{class}:{method} - Starting checking token and possible refresh", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
        await _tokenConfig.CookieEvents.ValidatePrincipal(context);

        var tokens = context.Properties.GetTokens()?.ToList();
        if (tokens == null || !tokens.Any())
        {
            _logger.LogError("{class}:{method} -No tokens found in cookie properties. SaveTokens must be enabled for automatic token refresh.", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
            return;
        }

        var refreshToken = tokens.SingleOrDefault(t => t.Name == OpenIdConnectParameterNames.RefreshToken);
        if (refreshToken == null)
        {
            _logger.LogError("{class}:{method} -No refresh token found in cookie properties. A refresh token must be requested and SaveTokens must be enabled.", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
            return;
        }

        var expiresAt = tokens.SingleOrDefault(t => t.Name == "expires_at");
        if (expiresAt == null)
        {
            _logger.LogError("{class}:{method} -No expires_at value found in cookie properties.", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
            return;
        }
        var dtExpires = DateTimeOffset.Parse(expiresAt.Value, CultureInfo.InvariantCulture);
        var rfValue = refreshToken.Value;
        _refreshTokenStore.AddIfNotExist(rfValue, null, user);

        if (!_refreshTokenStore.IsLatest(rfValue, user))
        {
            //var latesttoken = refreshTokenStore.GetLatestToken;
            //rfValue = latesttoken.CurrentToken;
            //dtExpires = latesttoken.ExpireAt;
            //_logger.LogTrace("{class}:{method} - Using latest token {token} expires at {expires}", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal),rfValue,dtExpires);
        }
        else
        {
            _logger.LogTrace("{class}:{method} - Using current token {token} expires at {expires}", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal), rfValue, dtExpires);
        }

        var dtRefresh = dtExpires.Subtract(_tokenConfig.RefreshBeforeExpiration); //.Subtract(new TimeSpan(0,7,0)); // For testing it faster
        _logger.LogTrace("ValidatePrincipal: expires_at: {dtExpires}, refresh_before: {refreshBeforeExpiration}, refresh_at: {dtRefresh}, now: {utcNow}, refresh_token: {refreshToken}, NoOfTokensInStore {noOfRefreshTokens}", dtExpires, _tokenConfig.RefreshBeforeExpiration, dtRefresh, _clock.UtcNow, refreshToken.Value, _refreshTokenStore.RefreshTokens.Count);
        if (_helseIdConfig.UseApis)
        {
            if (dtRefresh < _clock.UtcNow)
            {
                var shouldRefresh = PendingRefreshTokenRequests.TryAdd(rfValue, true);
                _logger.LogTrace("{class}:{method} - Should refresh: {shouldRefresh}, No Of tokens in PendingRefreshTokenRequests {count} ", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal), shouldRefresh, PendingRefreshTokenRequests.Count);
                if (shouldRefresh)
                {
                    try
                    {
                        var response = await _service.RefreshTokenAsync(rfValue);
                        if (response.IsError)
                        {
                            _logger.LogTrace("Error refreshing token: {error}, {errordesc}\n{json}", response.Error, response.ErrorDescription, response.Json);
                            _refreshTokenStore.Dump();
                            context.RejectPrincipal();
                            return;
                        }
                        _refreshTokenStore.Add(rfValue, response, user);
                        var newExpiresAt = context.UpdateTokens(response);
                        _logger.LogTrace("{class}.{method} - SignInAsync now as it expires at: {newExpiresAt}", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal), newExpiresAt);
                        _logger.LogTrace("{class}.{method} - Refresh tokens: Current {current}, New {new}", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal), rfValue, response.RefreshToken);
                        context.ShouldRenew = true;

                        await context.HttpContext.SignInAsync(context.Principal, context.Properties);
                    }
                    finally
                    {
                        PendingRefreshTokenRequests.TryRemove(rfValue, out _);
                    }
                }
                else
                {
                    _logger.LogTrace("{class}:{method} -: Refresh token already requested, value {refreshtoken}", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal), refreshToken.Value);
                }
            }
            else
            {
                _logger.LogTrace("{class}:{method} -: No need to refresh token yet.", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
            }
        }
        else
        {
            _logger.LogTrace("{class}:{method} - No Apis in configuration", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
        }

        _logger.LogTrace("{class}:{method} finished", nameof(AutomaticTokenManagementCookieEvents), nameof(ValidatePrincipal));
    }

    public override async Task SigningOut(CookieSigningOutContext context)
    {
        _logger.LogTrace("{class}:{method}: SigningOut", nameof(AutomaticTokenManagementCookieEvents), nameof(SigningOut));
        await _tokenConfig.CookieEvents.SigningOut(context);

        if (!_tokenConfig.RevokeRefreshTokenOnSignout)
            return;

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
        }
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        return _tokenConfig.CookieEvents.RedirectToAccessDenied(context);
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        return _tokenConfig.CookieEvents.RedirectToLogin(context);
    }

    public override Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context)
    {
        return _tokenConfig.CookieEvents.RedirectToLogout(context);
    }

    public override Task RedirectToReturnUrl(RedirectContext<CookieAuthenticationOptions> context)
    {
        return _tokenConfig.CookieEvents.RedirectToReturnUrl(context);
    }

    public override Task SignedIn(CookieSignedInContext context)
    {
        _logger.LogTrace("{class}:{method}", nameof(AutomaticTokenManagementCookieEvents), nameof(SignedIn));
        return _tokenConfig.CookieEvents.SignedIn(context);
    }

    public override Task SigningIn(CookieSigningInContext context)
    {
        _logger.LogTrace("{class}:{method}", nameof(AutomaticTokenManagementCookieEvents), nameof(SigningIn));
        return _tokenConfig.CookieEvents.SigningIn(context);
    }
}

/// <summary>
/// ICurrentUser resolved from cookie stored Identity
/// </summary>
public class UserByIdentity : ICurrentUser
{
    public UserByIdentity(ClaimsIdentity? identity)
    {
        if (identity == null)
        {
            return;
        }
        Name = identity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
        PidPseudonym = identity.Claims.SingleOrDefault(c => c.Type == "pid_pseudonym")?.Value ?? "";
        Id = identity.Claims.SingleOrDefault(c => c.Type == "id")?.Value ?? "";
        HprNummer = identity.Claims.SingleOrDefault(c => c.Type == "hpr_nummer")?.Value ?? "";
        Pid = identity.Claims.SingleOrDefault(c => c.Type == "pid")?.Value ?? "";
        SecurityLevel = identity.Claims.SingleOrDefault(c => c.Type == "security_level")?.Value ?? "";
        AssuranceLevel = identity.Claims.SingleOrDefault(c => c.Type == "assurance_level")?.Value ?? "";
        Network = identity.Claims.SingleOrDefault(c => c.Type == "network")?.Value ?? "";
    }

    public string Id { get; } = "";
    public string Name { get; } = "";
    public string HprNummer { get; } = "";
    public string PidPseudonym { get; } = "";
    public string Pid { get; } = "";
    public string SecurityLevel { get; } = "";
    public string AssuranceLevel { get; } = "";
    public string Network { get; } = "";
}