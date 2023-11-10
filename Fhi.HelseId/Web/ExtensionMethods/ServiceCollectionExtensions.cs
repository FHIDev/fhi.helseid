using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Web.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Use this to add the HelseId Api access token handling to the app.
    /// It will honor the AuthUse, so httpclients will be set up with or without token handling.
    /// Retrieve the services using the Name property
    /// </summary>
    public static HelseIdWebAuthBuilder AddOutgoingApis(this IServiceCollection _)
    {
        return HelseIdWebAuthBuilderExtensions.AuthBuilder.AddOutgoingApiServices();
    }
    
}