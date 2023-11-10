using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Infrastructure;
using Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseId.Common
{
    /// <summary>
    /// This is to be used with either User or Client credentials
    /// See https://github.com/reactiveui/refit#bearer-authentication  
    /// </summary>
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly ILogger<AuthHeaderHandler> logger;
        private readonly IRefreshTokenStore refreshTokenStore;
        private readonly ICurrentUser user;

        public AuthHeaderHandler(IHttpContextAccessor contextAccessor
            ,ILogger<AuthHeaderHandler> logger
            , IRefreshTokenStore refreshTokenStore
            ,ICurrentUser user)
        {
            logger.LogMember();
            this.contextAccessor = contextAccessor;
            this.logger = logger;
            this.refreshTokenStore = refreshTokenStore;
            this.user = user;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = contextAccessor.HttpContext ?? throw new NoContextException();
            logger.LogTrace("{class}.{method} - Starting", nameof(AuthHeaderHandler), nameof(SendAsync));
            var token = await ctx.GetUserAccessTokenAsync(cancellationToken: cancellationToken);

            if (refreshTokenStore.GetLatestToken(user)!=null && !string.IsNullOrEmpty(refreshTokenStore.GetLatestToken(user)?.AccessToken) && refreshTokenStore.GetLatestToken(user)?.AccessToken != token)
                token = refreshTokenStore.GetLatestToken(user)?.AccessToken;
            if (token == null)
            {
                logger.LogError("{class}.{method} No access token found in context. Make sure you have added the AddTokenManagement() to your Startup.cs", nameof(AuthHeaderHandler), nameof(SendAsync));
            }
            else
            {
               logger.LogTrace("{class}.{method} - Found access token in context (hash:{hash})", nameof(AuthHeaderHandler), nameof(SendAsync),token.GetHashCode());
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("{class}.{method} Request to {url} failed with status code {statusCode}", nameof(AuthHeaderHandler), nameof(SendAsync),request.RequestUri, response.StatusCode);
            }

            return response;
        }
    }

    /// <summary>
    /// This is to be used for Apis that need to send same access tokens onwards to further Apis
    /// See https://github.com/reactiveui/refit#bearer-authentication
    /// </summary>
    public class AuthHeaderHandlerForApi : DelegatingHandler
    {
        private readonly IHttpContextAccessor contextAccessor;
        public AuthHeaderHandlerForApi(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = contextAccessor.HttpContext ?? throw new NoContextException();
            var token = await ctx.AccessToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",  token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    [Serializable]
    public class NoContextException: Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NoContextException()
        {
        }

        public NoContextException(string message) : base(message)
        {
        }

        public NoContextException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NoContextException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
