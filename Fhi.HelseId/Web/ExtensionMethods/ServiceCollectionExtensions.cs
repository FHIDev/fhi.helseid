using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web.Hpr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;



namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddHelseIdAuthentication(this IServiceCollection services)
        {
            var builder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = HelseIdContext.Scheme;
            });
            return builder;
        }

        public static (AuthorizationPolicy AuthPolicy,string PolicyName) AddHelseIdAuthorizationPolicy(this IServiceCollection services, bool useHpr, bool useHprNumber)
        {
            var authenticatedHidUserPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(IdentityClaims.SecurityLevel, SecurityLevel.Level4)
                .Build();
            if (useHprNumber)
            {
                var hprNumberPolicy = new AuthorizationPolicyBuilder()
                    .Combine(authenticatedHidUserPolicy)
                    .RequireAssertion(context => context.User.HprNumber() != null)
                    .Build();
                if (useHpr)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .Combine(hprNumberPolicy);
                    policy.Requirements.Add(new LegeAuthorizationRequirement());
                    var legePolicy = policy.Build();
                    services.AddAuthorization(config =>
                    {
                        config.AddPolicy(Policies.HidAuthenticated, authenticatedHidUserPolicy);
                        config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                        config.AddPolicy(Policies.LegePolicy, legePolicy);
                        config.DefaultPolicy = legePolicy;
                    });
                    return (legePolicy, Policies.LegePolicy);
                }

                services.AddAuthorization(config =>
                {
                    config.AddPolicy(Policies.HidAuthenticated, authenticatedHidUserPolicy);
                    config.AddPolicy(Policies.HprNummer, hprNumberPolicy);
                    config.DefaultPolicy = hprNumberPolicy;
                });
                return (hprNumberPolicy, Policies.HprNummer);
            }

            services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.HidAuthenticated, authenticatedHidUserPolicy);
                config.DefaultPolicy = authenticatedHidUserPolicy;
            });
            return (authenticatedHidUserPolicy, Policies.HidAuthenticated);


        }

    }
}
