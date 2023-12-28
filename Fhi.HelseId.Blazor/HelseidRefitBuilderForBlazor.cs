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
        private readonly WebApplicationBuilder builder;
        private readonly HelseIdWebKonfigurasjon config;
        private List<Type> DelegationHandlers = new();

        public RefitSettings RefitSettings { get; set; }

        public HelseidRefitBuilderForBlazor(WebApplicationBuilder builder, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings)
        {
            this.RefitSettings = refitSettings ?? CreateRefitSettings();

            this.builder = builder;
            this.config = config;

            builder.AddStateHandlers().AddScopedState<HelseIdState>();

            builder.Services.AddScoped<BlazorContextHandler>();
            builder.Services.AddScoped<BlazortContextMiddleware>();
            builder.Services.AddScoped<BlazorTokenService>();

            AddHandler<BlazorTokenHandler>();
        }

        public HelseidRefitBuilderForBlazor AddHandler<T>() where T : DelegatingHandler
        {
            DelegationHandlers.Add(typeof(T));
            builder.Services.AddTransient<T>();
            return this;
        }

        public HelseidRefitBuilderForBlazor ClearHandlers()
        {
            DelegationHandlers.Clear();
            return this;
        }

        public HelseidRefitBuilderForBlazor AddRefitClient<T>(string? nameOfService = null, Func<IHttpClientBuilder, IHttpClientBuilder>? extra = null) where T : class
        {
            var name = nameOfService ?? typeof(T).Name;

            // We are using a custom factory, since the Refit factory does not created correctly scoped TokenHandlers.
            // We need a new TokenHandler for each request to get the access token from the correct context.
            builder.Services.AddScoped((s) =>
            {
                var client = CreateHttpClient(s, DelegationHandlers);
                client.BaseAddress = config.UriToApiByName(name);
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
