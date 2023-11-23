using Fhi.HelseId.Common;
using Fhi.HelseId.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Fhi.HelseId.Refit;

public class RefitBuilder
{
    private readonly WebApplicationBuilder builder;
    readonly string correlationId;
    readonly HelseIdWebKonfigurasjon webConfig;

    public RefitBuilder(WebApplicationBuilder builder)
    {
        this.builder = builder;
        correlationId = Guid.NewGuid().ToString();
        builder.Services.AddHeaderPropagation(options => options.Headers.Add("x-correlation-id"));
        webConfig = builder.Configuration.GetSection(nameof(HelseIdWebKonfigurasjon)).Get<HelseIdWebKonfigurasjon>() ?? throw new MissingConfigurationException(nameof(HelseIdWebKonfigurasjon));

    }

    public RefitBuilder AddRefitClient<T>(string nameOfService,Func<IHttpClientBuilder,IHttpClientBuilder>? extra=null) where T : class
    {
        var clientBuilder = builder.Services.AddRefitClient<T>()
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = webConfig.UriToApiByName(nameOfService);
                httpClient.DefaultRequestHeaders.Add("x-correlation-id", correlationId);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddHeaderPropagation();
        extra?.Invoke(clientBuilder);
        return this;
    }

}

public class MissingConfigurationException : Exception
{
    public MissingConfigurationException(string outgoingApisName) : base(outgoingApisName)
    {

    }
}



public static class Extensions
{
    public static RefitBuilder AddRefitbuilder(this WebApplicationBuilder builder)
    {
        return new RefitBuilder(builder);
    }
    
}