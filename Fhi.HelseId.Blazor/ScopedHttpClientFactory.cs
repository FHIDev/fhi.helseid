using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public interface IScopedHttpClientFactory
    {
        HttpClient CreateHttpClient(string name, IServiceProvider provider, List<Type> DelegationHandlers);
    }

    public class ScopedHttpClientFactory : IScopedHttpClientFactory
    {
        public Func<string, HttpClientHandler> HttpClientHandlerBuilder { get; set; } = (name) => new HttpClientHandler();

        public bool DisposeHandlers { get; set; } = true;

        public ScopedHttpClientFactory()
        {
        }

        public HttpClient CreateHttpClient(string name, IServiceProvider provider, List<Type> DelegationHandlers)
        {
            if (DelegationHandlers.Count == 0)
            {
                return new HttpClient();
            }

            var mainHandler = (DelegatingHandler)provider.GetRequiredService(DelegationHandlers.First());
            var outer = mainHandler;
            foreach (var handlerType in DelegationHandlers.Skip(1))
            {
                if (outer.InnerHandler != null)
                {
                    throw new Exception($"{outer.GetType()} should not have a inner handler when.");
                }

                outer.InnerHandler = (DelegatingHandler)provider.GetRequiredService(handlerType);
                outer = (DelegatingHandler)outer.InnerHandler;
            }

            outer.InnerHandler = HttpClientHandlerBuilder.Invoke(name);

            // Attempt to clean up handlers. They may still linger awaiting for closing of sockets
            return new HttpClient(mainHandler, DisposeHandlers);
        }
    }
}
