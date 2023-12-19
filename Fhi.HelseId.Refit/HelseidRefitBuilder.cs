using Fhi.HelseId.Web;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Fhi.HelseId.Common;

namespace Fhi.HelseId.Refit
{
    public class HelseidRefitBuilder
    {
        private readonly WebApplicationBuilder builder;
        private readonly HelseIdWebKonfigurasjon config;
        private List<Type> DelegationHandlers = new();

        public RefitSettings RefitSettings { get; set; }

        public HelseidRefitBuilder(WebApplicationBuilder builder, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings)
        {
            this.RefitSettings = refitSettings ?? CreateRefitSettings();

            this.builder = builder;
            this.config = builder.Configuration.GetSection(nameof(HelseIdWebKonfigurasjon)).Get<HelseIdWebKonfigurasjon>() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));

            AddHandler<AuthHeaderHandler>();
        }

        public HelseidRefitBuilder AddHandler<T>() where T : DelegatingHandler
        {
            DelegationHandlers.Add(typeof(T));
            builder.Services.AddTransient<T>();
            return this;
        }

        public HelseidRefitBuilder ClearHandlers()
        {
            DelegationHandlers.Clear();
            return this;
        }

        /// <summary>
        /// Adds propagation and handling of correlation ids. You should add this before any logging-delagates. Remember to add "app.UseHeaderPropagation()" in your startup code
        /// </summary>
        /// <returns></returns>
        public HelseidRefitBuilder AddCorrelationId()
        {
            AddHandler<CorrelationIdHandler>();

            builder.Services.AddHeaderPropagation(o =>
            {
                o.Headers.Add(CorrelationIdHandler.CorrelationIdHeaderName, context => string.IsNullOrEmpty(context.HeaderValue) ? Guid.NewGuid().ToString() : context.HeaderValue);
            });

            return this;
        }

        public HelseidRefitBuilder AddRefitClient<T>(string? nameOfService = null, Func<IHttpClientBuilder, IHttpClientBuilder>? extra = null) where T : class
        {
            var name = nameOfService ?? typeof(T).Name;

            var clientBuilder = builder.Services.AddRefitClient<T>()
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = config.UriToApiByName(name);
                })
                .AddHeaderPropagation();

            foreach (var type in DelegationHandlers)
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
