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
        private readonly List<Type> delegationHandlers = new();
        private readonly HelseidRefitBuilderOptions options = new();
        private bool hasAddedHeaderEncoding;

        private RefitSettings RefitSettings { get; }

        /// <summary>
        /// Creates a new instance of HelseidRefitBuilder. This is the main entry point for setting up Refit clients
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config">Uses only the UriToApiByName method</param>
        /// <param name="refitSettings">Optional. If not specified uses a default serialization setup with CamelCase, Ignorecase and enumconvertion</param>
        public HelseidRefitBuilder(IServiceCollection services, HelseIdWebKonfigurasjon config, RefitSettings? refitSettings=null)
        {
            RefitSettings = refitSettings ?? CreateRefitSettings();

            this.services = services;
            this.config = config;

            services.AddSingleton(options);
            services.AddHttpContextAccessor();

            AddHandler<AuthHeaderHandler>();
        }

        /// <summary>
        /// Add delegating handlers to the Refit client. Also adds the handler to the service collection as Transient
        /// </summary>
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

        /// <summary>
        /// Adds a Refit client interface and which service name to bind to, and an optional extra configuration
        /// </summary>
        /// <typeparam name="T">The Refit interface definition for the Api</typeparam>
        /// <param name="nameOfService">Name of the service that will serve the Refit Api</param>
        /// <param name="extra"></param>
        /// <returns></returns>
        public HelseidRefitBuilder AddRefitClient<T>(string? nameOfService = null, Func<IHttpClientBuilder, IHttpClientBuilder>? extra = null) where T : class
        {
            if (!hasAddedHeaderEncoding)
            {
                hasAddedHeaderEncoding = true;
                AddHandler<FhiHeaderDelegationHandler>();
            }

            var name = nameOfService ?? typeof(T).Name;

            var clientBuilder = services.AddRefitClient<T>(RefitSettings)
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
