using Fhi.HelseId.Web;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public class HelseidRefitBuilderForBlazor
    {
        private readonly IServiceCollection services;
        private readonly HelseIdWebKonfigurasjon helseIdConfig;
        private List<Type> DelegationHandlers = new();
        private HelseidRefitBuilderForBlazorOptions options = new HelseidRefitBuilderForBlazorOptions();
        private ScopedHttpClientFactory Factory = new ScopedHttpClientFactory();

        public RefitSettings RefitSettings { get; set; }

        public HelseidRefitBuilderForBlazor(IServiceCollection services, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings)
        {
            this.RefitSettings = refitSettings ?? CreateRefitSettings();

            this.services = services;
            this.helseIdConfig = config;

            services.AddStateHandlers().AddScopedState<HelseIdState>();

            services.AddScoped<BlazorContextHandler>();
            services.AddScoped<BlazortContextMiddleware>();
            services.AddScoped<BlazorTokenService>();
            services.AddSingleton(options);

            services.AddSingleton<IScopedHttpClientFactory>(Factory);

            AddHandler<BlazorTokenHandler>();
        }

        public HelseidRefitBuilderForBlazor AddHandler<T>() where T : DelegatingHandler
        {
            DelegationHandlers.Add(typeof(T));
            services.AddTransient<T>();
            return this;
        }

        public HelseidRefitBuilderForBlazor SetHttpClientHandlerBuilder(Func<string, HttpClientHandler> builder)
        {
            Factory.HttpClientHandlerBuilder = builder;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispose">true if the inner handler should be disposed of by HttpClient.Dispose; if you intend to reuse the inner handler.</param>
        /// <returns></returns>
        public HelseidRefitBuilderForBlazor DisposeHandleres(bool dispose)
        {
            Factory.DisposeHandlers = dispose;
            return this;
        }

        public HelseidRefitBuilderForBlazor ClearHandlers()
        {
            DelegationHandlers.Clear();
            return this;
        }

        /// <summary>
        /// To be able to access the http context to log out of a blazor app
        /// we need to do this from middleware where the HttpContext is availible.
        /// To trigger it, be sure to force a reload when navigating to the logout url
        /// f.ex:  NavManager.NavigateTo($"/logout", forceLoad: true);
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="url">The url used to trigger logging out.</param>
        /// <param name="redirect">The url to continue to after logging out.</param>
        /// <returns></returns>

        public HelseidRefitBuilderForBlazor ConfigureLogout(bool enabled = true, string url = "/logout", string redirect = "/loggedout")
        {
            options.UseLogoutUrl = enabled;
            options.LogOutUrl = url;
            options.LoggedOutRedirectUrl = redirect;
            return this;
        }

        /// <summary>
        /// Adds propagation and handling of correlation ids. You should add this before any logging-delagates. Remember to add "app.UseHeaderPropagation()" in your startup code.
        /// </summary>
        /// <returns></returns>
        public HelseidRefitBuilderForBlazor AddCorrelationId()
        {
            options.UseCorrelationId = true;

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

            services.AddScoped((s) =>
            {
                // We are using a custom factory, since the Refit factory does not created correctly scoped TokenHandlers.
                // We need a new TokenHandler for each request to get the access token from the correct context.
                // This is BAD, as we are not using the HttpClientFactory to create the HttpClients.
                var client = s.GetRequiredService<IScopedHttpClientFactory>().CreateHttpClient(name, s, DelegationHandlers);
                client.BaseAddress = helseIdConfig.UriToApiByName(name);
                extra?.Invoke(client);
                return RestService.For<T>(client, RefitSettings);
            });

            return this;
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
