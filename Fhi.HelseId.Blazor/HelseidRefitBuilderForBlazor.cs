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
        private HelseidRefitBuilderForBlazorConfig options = new HelseidRefitBuilderForBlazorConfig();

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

            // We are using a custom factory, since the Refit factory does not created correctly scoped TokenHandlers.
            // We need a new TokenHandler for each request to get the access token from the correct context.
            services.AddScoped((s) =>
            {
                var client = CreateHttpClient(s, DelegationHandlers);
                client.BaseAddress = helseIdConfig.UriToApiByName(name);
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
