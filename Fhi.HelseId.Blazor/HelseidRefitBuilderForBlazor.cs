using Fhi.HelseId.Web;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public class HelseidRefitBuilderForBlazor
    {
        private readonly IServiceCollection services;
        private readonly HelseIdWebKonfigurasjon config;
        private List<Type> DelegationHandlers = new();

        public RefitSettings RefitSettings { get; set; }

        public HelseidRefitBuilderForBlazor(IServiceCollection services, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings)
        {
            this.RefitSettings = refitSettings ?? CreateRefitSettings();

            this.services = services;
            this.config = config;

            services.AddStateHandlers().AddScopedState<HelseIdState>();

            services.AddScoped<BlazorContextHandler>();
            services.AddScoped<BlazortContextMiddleware>();
            services.AddScoped<BlazorTokenService>();

            AddHandler<BlazorTokenHandler>();
        }

        public HelseidRefitBuilderForBlazor AddHandler<T>() where T : DelegatingHandler
        {
            DelegationHandlers.Add(typeof(T));
            services.AddTransient<T>();
            return this;
        }

        public HelseidRefitBuilderForBlazor ClearHandlers()
        {
            DelegationHandlers.Clear();
            return this;
        }

        /// <summary>
        /// Adds propagation and handling of correlation ids. You should add this before any logging-delagates. Remember to add "app.UseHeaderPropagation()" in your startup code.
        /// </summary>
        /// <returns></returns>
        public HelseidRefitBuilderForBlazor AddCorrelationId()
        {
            AddHandler<CorrelationIdHandler>();

            services.AddHeaderPropagation(o =>
            {
                o.Headers.Add(CorrelationIdHandler.CorrelationIdHeaderName, context => string.IsNullOrEmpty(context.HeaderValue) ? Guid.NewGuid().ToString() : context.HeaderValue);
            });

            return this;
        }

        public HelseidRefitBuilderForBlazor AddRefitClient<T>(string? nameOfService = null, Func<HttpClient, HttpClient>? extra = null) where T : class
        {
            var name = nameOfService ?? typeof(T).Name;

            // We are using a custom factory, since the Refit factory does not created correctly scoped TokenHandlers.
            // We need a new TokenHandler for each request to get the access token from the correct context.
            services.AddScoped((s) =>
            {
                var client = CreateHttpClient(s, DelegationHandlers);
                client.BaseAddress = config.UriToApiByName(name);
                extra?.Invoke(client);
                return RestService.For<T>(client, RefitSettings);
            });

            return this;
        }

        private static HttpClient CreateHttpClient(IServiceProvider provider, List<Type> DelegationHandlers)
        {
            if (DelegationHandlers.Count == 0)
            {
                return new HttpClient();
            }

            var mainHandler = (DelegatingHandler)provider.GetRequiredService(DelegationHandlers.First());
            var outer = mainHandler;
            foreach (var handlerType in DelegationHandlers.Skip(1))
            {
                var i = 0;
                while (outer.InnerHandler != null)
                {
                    outer = (DelegatingHandler)outer.InnerHandler;
                    if (i++ > 1000) throw new Exception($"Circular reference of inner handlers in handler: {outer.GetType()}");
                }

                outer.InnerHandler = (DelegatingHandler)provider.GetRequiredService(handlerType);
                outer = (DelegatingHandler)outer.InnerHandler;
            }

            outer.InnerHandler = new HttpClientHandler();

            return new HttpClient(mainHandler);
        }

        private static RefitSettings CreateRefitSettings()
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            var refitSettings = new RefitSettings()
            {
                ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions),
            };

            return refitSettings;
        }
    }
}
