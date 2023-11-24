using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Web.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Use this to add the HelseId Api access token prerequisites to the app.
    /// Use either 
    /// </summary>
    public static HelseIdWebAuthBuilder AddOutgoingApis(this IServiceCollection _)
    {
        return HelseIdWebAuthBuilderExtensions.AuthBuilder.AddOutgoingApiServices();
    }
    
}