using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Common.ExtensionMethods;
using Fhi.HelseId.Common.Exceptions;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Infrastructure;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fhi.HelseId.Common;

/// <summary>
/// This is to be used with either User or Client credentials
/// See https://github.com/reactiveui/refit#bearer-authentication  
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class AuthHeaderHandler : DelegatingHandler
{
    public const string AnonymousOptionKey = "Anonymous";

    private readonly IHttpContextAccessor contextAccessor;
    private readonly ILogger<AuthHeaderHandler> logger;
    private readonly IRefreshTokenStore refreshTokenStore;
    private readonly ICurrentUser user;
    private readonly HelseIdWebKonfigurasjon config;

    public AuthHeaderHandler(IHttpContextAccessor contextAccessor,
        ILogger<AuthHeaderHandler> logger,
        IRefreshTokenStore refreshTokenStore,
        ICurrentUser user,
        IOptions<HelseIdWebKonfigurasjon> options)
    {
        config = options.Value;
        logger.LogMember();
        this.contextAccessor = contextAccessor;
        this.logger = logger;
        this.refreshTokenStore = refreshTokenStore;
        this.user = user;
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Options.Any(x => x.Key == AnonymousOptionKey))
        {
            logger.LogTrace("{class}.{method} - Skipping Access token because of anonymous HttpRequestMessage options", nameof(AuthHeaderHandler), nameof(SendAsync));
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var ctx = contextAccessor.HttpContext ?? throw new NoContextException();
        logger.LogTrace("{class}.{method} - Starting", nameof(AuthHeaderHandler), nameof(SendAsync));
        var token = await ctx.GetUserAccessTokenAsync(cancellationToken: cancellationToken);

        if (config.UseRefreshTokenStore && refreshTokenStore.GetLatestToken(user) != null && !string.IsNullOrEmpty(refreshTokenStore.GetLatestToken(user)?.AccessToken) && refreshTokenStore.GetLatestToken(user)?.AccessToken != token)
            token = refreshTokenStore.GetLatestToken(user)?.AccessToken;
        if (token == null)
        {
            logger.LogError("{class}.{method} No access token found in context. Make sure you have added the AddTokenManagement() to your Startup.cs", nameof(AuthHeaderHandler), nameof(SendAsync));
        }
        else
        {
            logger.LogTrace("{class}.{method} - Found access token in context (hash:{hash})", nameof(AuthHeaderHandler), nameof(SendAsync), token.GetHashCode());
        }
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("{class}.{method} Request to {url} failed with status code {statusCode}", nameof(AuthHeaderHandler), nameof(SendAsync), request.RequestUri, response.StatusCode);
        }

        return response;
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
