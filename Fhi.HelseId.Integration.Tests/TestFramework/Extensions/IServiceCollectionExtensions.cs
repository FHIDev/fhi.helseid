using Fhi.TestFramework.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.TestFramework.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        internal static IServiceCollection AddFakeTestAuthenticationScheme(this IServiceCollection services, string accessToken, string idToken)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = "TestSignin";
                options.DefaultAuthenticateScheme = "TestSignin";
            }).AddTestAuthentication("TestSignin", accessToken, idToken);

            return services;
        }
    }
}
