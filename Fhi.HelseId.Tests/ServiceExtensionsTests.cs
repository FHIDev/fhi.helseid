using System.Linq;
using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    public class ServiceExtensionsTests
    {
        [TestCase(true,true,Policies.GodkjentHprKategoriPolicy)]
        [TestCase(true, false, Policies.HprNummer)]
        [TestCase(false, true, Policies.HidOrApi)]
        [TestCase(false, false, Policies.HidOrApi)]
        public void ThatAddingAuthorizationPoliciesHonourFeatureflags(bool useHprNumber, bool useHpr,
            string expectedPolicyName)
        {
            var helseIdFeatures = Substitute.For<IHelseIdHprFeatures>();
            helseIdFeatures.UseHprNumber.Returns(useHprNumber);
            var sc = Substitute.For<IServiceCollection>();
            var hprFeatures = Substitute.For<IHprFeatureFlags>();
            hprFeatures.UseHpr.Returns(useHpr);
            hprFeatures.UseHprPolicy.Returns(useHpr);
            var wl = Substitute.For<IWhitelist>();
            var helseIdWebConfig = Substitute.For<IHelseIdWebKonfigurasjon>();

            var sut = sc.AddHelseIdAuthorizationPolicy(helseIdFeatures, hprFeatures, helseIdWebConfig, wl);
            Assert.That(sut.PolicyName, Is.EqualTo(expectedPolicyName));
        }
    }
}
