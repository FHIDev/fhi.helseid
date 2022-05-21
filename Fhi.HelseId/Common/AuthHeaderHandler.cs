using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Fhi.HelseId.Common
{
    /// <summary>
    /// This is to be used with either User or Client credentials
    /// See https://github.com/reactiveui/refit#bearer-authentication
    /// </summary>
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public AuthHeaderHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _contextAccessor.HttpContext.GetUserAccessTokenAsync(false, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// This is to be used for Apis that need to send same access tokens onwards to further Apis
    /// See https://github.com/reactiveui/refit#bearer-authentication
    /// </summary>
    public class AuthHeaderHandlerForApi : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public AuthHeaderHandlerForApi(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _contextAccessor.HttpContext.GetTokenAsync("access_token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
