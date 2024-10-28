using System.Globalization;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Microsoft.AspNetCore.Authentication;

namespace Fhi.HelseId.Blazor;

public interface IBlazorTokenService
{
    Task<string?> GetToken();
}

/// <summary>
/// Service for getting valid access tokens.
/// </summary>
public class BlazorTokenService : IBlazorTokenService
{
    private const int CLOSE_TO_EXPIRE_SECONDS = 5; // Account for IO-delay from ExpiresIn is set externally until it is read

    private readonly HelseIdState _state;
    private readonly TokenEndpointService _tokenRefreshService;
    private readonly BlazorContextService _contextHandler;

    public BlazorTokenService(HelseIdState state, TokenEndpointService tokenRefreshService, BlazorContextService contextHandler)
    {
        _state = state;
        _tokenRefreshService = tokenRefreshService;
        _contextHandler = contextHandler;
    }

    public async Task<string?> GetToken()
    {
        if (!_state.HasBeenInitialized)
        {
            throw new Exception("HelseIdState has not been populated. Have you remembered to wrap your App.razor code in <CascadingStates>...</CascadingStates>?");
        }

        // check if the token is expired (or close to expiring)
        if (_state.TokenExpires < DateTime.UtcNow.AddSeconds(-CLOSE_TO_EXPIRE_SECONDS))
        {
            try
            {
                var t = await _tokenRefreshService.RefreshTokenAsync(_state.RefreshToken);
                if (t.IsError)
                {
                    throw new Exception($"Unable to refresh token: {t.Error}");
                }

                _state.AccessToken = t.AccessToken!;
                _state.TokenExpires = t.ExpiresAt;
                _state.RefreshToken = t.RefreshToken!;

                await UpdateContext();
            }
            catch
            {
                await SignOut();
                throw;
            }
        }

        return _state.AccessToken;
    }

    private async Task UpdateContext()
    {
        await _contextHandler.NewContext(async context =>
        {
            var auth = await context.AuthenticateAsync()!;
            if (!auth.Succeeded)
            {
                await context.SignOutAsync();
                return;
            }

            auth.Properties.UpdateTokenValue("access_token", _state.AccessToken);
            auth.Properties.UpdateTokenValue("refresh_token", _state.RefreshToken);
            auth.Properties.UpdateTokenValue("expires_at", _state.TokenExpires.ToString("o", CultureInfo.InvariantCulture));

            await context.SignInAsync(auth.Principal, auth.Properties);
        });
    }

    private async Task SignOut()
    {
        await _contextHandler.NewContext(async context =>
        {
            await context.SignOutAsync();
        });
    }
}