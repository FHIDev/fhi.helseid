using Microsoft.AspNetCore.Authentication;
using System.Globalization;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;

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
    private const int CLOSE_TO_EXPIRE_SECONDS = 5; // acount for IO-delay from ExpiresIn is set extyernally until it is read

    private readonly HelseIdState state;
    private readonly TokenEndpointService tokenRefreshService;
    private readonly BlazorContextService contextHandler;

    public BlazorTokenService(HelseIdState state, TokenEndpointService tokenRefreshService, BlazorContextService contextHandler)
    {
        this.state = state;
        this.tokenRefreshService = tokenRefreshService;
        this.contextHandler = contextHandler;
    }

    public async Task<string?> GetToken()
    {
        if (!state.HasBeenInitialized)
        {
            throw new Exception("HelseIdState has not been populated. Have you remembered to wrap your App.razor code in <CascadingStates>...</CascadingStates>?");
        }

        // check if the token is expired (or close to expiring)
        if (state.TokenExpires < DateTime.UtcNow.AddSeconds(-CLOSE_TO_EXPIRE_SECONDS))
        {
            try
            {
                var t = await tokenRefreshService.RefreshTokenAsync(state.RefreshToken!);
                if (t.IsError)
                {
                    throw new Exception($"Unable to refresh token: {t.Error}");
                }

                state.AccessToken = t.AccessToken!;
                state.TokenExpires = DateTimeOffset.UtcNow.AddSeconds(t.ExpiresIn);
                state.RefreshToken = t.RefreshToken!;

                await UpdateContext();
            }
            catch
            {
                await SignOut();
                throw;
            }
        }

        return state.AccessToken;
    }

    private async Task UpdateContext()
    {
        await contextHandler.NewContext(async context =>
        {
            var auth = await context.AuthenticateAsync()!;
            if (!auth.Succeeded)
            {
                await context.SignOutAsync();
                return;
            }

            auth.Properties.UpdateTokenValue("access_token", state.AccessToken);
            auth.Properties.UpdateTokenValue("refresh_token", state.RefreshToken);
            auth.Properties.UpdateTokenValue("expires_at", state.TokenExpires.ToString("o", CultureInfo.InvariantCulture));

            await context.SignInAsync(auth.Principal, auth.Properties);
        });
    }

    private async Task SignOut()
    {
        await contextHandler.NewContext(async context =>
        {
            await context.SignOutAsync();
        });
    }
}

