﻿using Fhi.HelseId.Common.Identity;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Hpr;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    public class ServiceExtensionsTests
    {
        [TestCase(true,true,Policies.LegePolicy)]
        [TestCase(true, false, Policies.HprNummer)]
        [TestCase(false, true, Policies.HidAuthenticated)]
        [TestCase(false, false, Policies.HidAuthenticated)]
        public void ThatAddingAuthorizationPoliciesHonourFeatureflags(bool useHprNumber, bool useHpr,
            string expectedPolicyName)
        {
            var helseIdFeatures = Substitute.For<IHelseIdHprFeatures>();
            helseIdFeatures.UseHprNumber.Returns(useHprNumber);
            var sc = Substitute.For<IServiceCollection>();
            var hprFeatures = Substitute.For<IHprFeatureFlags>();
            hprFeatures.UseHpr.Returns(useHpr);
            hprFeatures.UseHprPolicy.Returns(useHpr);

            var sut = sc.AddHelseIdAuthorizationPolicy(helseIdFeatures, hprFeatures);

            Assert.That(sut.PolicyName, Is.EqualTo(expectedPolicyName));
        }
    }
}
