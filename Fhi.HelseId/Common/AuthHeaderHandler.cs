using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Web.Infrastructure;
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

        public AuthHeaderHandler(IHttpContextAccessor contextAccessor,ILogger<AuthHeaderHandler> logger)
        {
            this.contextAccessor = contextAccessor;
            this.logger = logger;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = contextAccessor.HttpContext;
            if (ctx == null)
                throw new NoContextException();
            var token = await ctx.GetUserAccessTokenAsync(cancellationToken: cancellationToken);
            if (token == null)
            {
                logger.LogError("No access token found in context. Make sure you have added the AddTokenManagement() to your Startup.cs");
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
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
            var ctx = contextAccessor.HttpContext;
            if (ctx == null)
                throw new NoContextException();
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
