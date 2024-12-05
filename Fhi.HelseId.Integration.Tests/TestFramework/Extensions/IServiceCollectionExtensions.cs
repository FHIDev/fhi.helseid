using System.Security.Claims;
using Fhi.TestFramework.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.TestFramework.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        private static string SchemeName => "FakeAuthentication";
        internal static IServiceCollection AddFakeTestAuthenticationScheme(this IServiceCollection services, string accessToken, string? idToken = null)
        {
            AddAuthentication(services).AddFakeAuthenticationScheme(SchemeName, accessToken, idToken);

            return services;
        }

        internal static IServiceCollection AddFakeTestAuthenticationScheme(this IServiceCollection services, IEnumerable<Claim> userClaims)
        {
            AddAuthentication(services).AddFakeAuthenticationScheme(SchemeName, userClaims);

            return services;
        }

        private static AuthenticationBuilder AddAuthentication(IServiceCollection services)
        {
            return services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = SchemeName;
                options.DefaultAuthenticateScheme = SchemeName;
            });
        }
    }
}
