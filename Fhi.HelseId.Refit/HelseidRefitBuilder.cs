using Fhi.HelseId.Web;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Fhi.HelseId.Common;

namespace Fhi.HelseId.Refit
{
    public class HelseidRefitBuilder
    {
        private readonly IServiceCollection services;
        private readonly HelseIdWebKonfigurasjon config;
        private List<Type> delegationHandlers = new();
        private readonly HelseidRefitBuilderOptions options = new HelseidRefitBuilderOptions();

        private RefitSettings refitSettings { get; set; }

        public HelseidRefitBuilder(IServiceCollection services, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings)
        {
            this.refitSettings = refitSettings ?? CreateRefitSettings();

            this.services = services;
            this.config = config;

            services.AddSingleton(options);

            AddHandler<AuthHeaderHandler>();
        }

        public HelseidRefitBuilder AddHandler<T>() where T : DelegatingHandler
        {
            delegationHandlers.Add(typeof(T));
            services.AddTransient<T>();
            return this;
        }

        public HelseidRefitBuilder ClearHandlers()
        {
            delegationHandlers.Clear();
            return this;
        }

        /// <summary>
        /// Adds propagation and handling of correlation ids. You should add this before any logging-delagates. Remember to add "app.UseHeaderPropagation()" in your startup code
        /// </summary>
        /// <returns></returns>
        public HelseidRefitBuilder AddCorrelationId()
        {
            options.UseCorrelationId = true;

            AddHandler<CorrelationIdHandler>();

            services.AddHeaderPropagation(o =>
            {
                o.Headers.Add(CorrelationIdHandler.CorrelationIdHeaderName, context => string.IsNullOrEmpty(context.HeaderValue) ? Guid.NewGuid().ToString() : context.HeaderValue);
            });

            return this;
        }

        public HelseidRefitBuilder AddRefitClient<T>(string? nameOfService = null, Func<IHttpClientBuilder, IHttpClientBuilder>? extra = null) where T : class
        {
            var name = nameOfService ?? typeof(T).Name;

            var clientBuilder = services.AddRefitClient<T>(refitSettings)
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = config.UriToApiByName(name);
                });

            if (options.UseCorrelationId)
            {
                clientBuilder.AddHeaderPropagation();
            }

            foreach (var type in delegationHandlers)
            {
                clientBuilder.AddHttpMessageHandler((s) => (DelegatingHandler)s.GetRequiredService(type));
            }

            extra?.Invoke(clientBuilder);

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
