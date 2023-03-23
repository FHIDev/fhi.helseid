using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Hpr;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    //public class ServiceExtensionsTests
    //{
    //    [TestCase(true,true,Policies.GodkjentHprKategoriPolicy)]
    //    [TestCase(true, false, Policies.HprNummer)]
    //    [TestCase(false, true, Policies.HidOrApi)]
    //    [TestCase(false, false, Policies.HidOrApi)]
    //    public void ThatAddingAuthorizationPoliciesHonourFeatureflags(bool useHprNumber, bool useHprPolicy,
    //        string expectedPolicyName)
    //    {
    //        var helseIdFeatures = Substitute.For<IHelseIdWebKonfigurasjon>();
    //        helseIdFeatures.UseHprNumber.Returns(useHprNumber);
    //        var sc = Substitute.For<IServiceCollection>();
    //        helseIdFeatures.UseHprNumber.Returns(useHprNumber);
    //        helseIdFeatures.UseHprPolicy.Returns(useHprPolicy);
            
    //        var sut = sc.AddHelseIdAuthorizationPolicy(helseIdFeatures);
    //        Assert.That(sut.PolicyName, Is.EqualTo(expectedPolicyName));
    //    }
    //}
}
