using System;
using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Worker;
using NUnit.Framework;
using MoreLinq;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class ClientIdConsistencyTests
    {
        private List<HelseIdWorkerKonfigurasjon> HelseIdWorkerConfigurasjonsForTests { get; } =
            new List<HelseIdWorkerKonfigurasjon>();

        private readonly SetupBaseConfigTests.AppSettingsUsage useOfAppsettings;

        private readonly HelseIdWorkerKonfigurasjon helseIdWorkerConfigurasjonForProduction = null!;

        /// <summary>
        /// Add the different types of appsettings you have for tests,  e.g. appsettings.test.json => test
        /// </summary>
        protected ClientIdConsistencyTests(List<string> types, SetupBaseConfigTests.AppSettingsUsage useOfAppsettings ,string prod="")
        {
            this.useOfAppsettings = useOfAppsettings;
            var fileNamesForTests = types.Select(o => $"appsettings.{o}.json");
            
            foreach (var fileName in fileNamesForTests)
            {
                var workerConfig = new HelseIdWorkerClientIds(fileName,useOfAppsettings).HelseIdWorkerKonfigurasjonUnderTest;
                HelseIdWorkerConfigurasjonsForTests.Add(workerConfig);
            }
            
            switch (useOfAppsettings)
            {
                case SetupBaseConfigTests.AppSettingsUsage.AppSettingsIsProd:
                    helseIdWorkerConfigurasjonForProduction = new HelseIdWorkerClientIds("appsettings.json", useOfAppsettings).HelseIdWorkerKonfigurasjonUnderTest;
                    break;
                case SetupBaseConfigTests.AppSettingsUsage.AppSettingsIsTestWhenDev:
                    break;
                case SetupBaseConfigTests.AppSettingsUsage.AppSettingsIsBaseOnly:
                    helseIdWorkerConfigurasjonForProduction = new HelseIdWorkerClientIds($"appsettings.{prod}.json", useOfAppsettings).HelseIdWorkerKonfigurasjonUnderTest;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(useOfAppsettings), useOfAppsettings, null);
            }
        }

        [Test]
        public void ThatClientIdsAreConsistent()
        {
            var clientIds = HelseIdWorkerConfigurasjonsForTests.DistinctBy(o => o.ClientId);
            var count = clientIds.Count();
            Assert.That(count, Is.EqualTo(1),
                $"The appsettings for test are using {count} different clientIds, instead of one");

        }

        [Test]
        public void ThatClientSecretsAreConsistent()
        {
            var clientSecrets = HelseIdWorkerConfigurasjonsForTests.DistinctBy(o => o.ClientSecret);
            var count = clientSecrets.Count();
            Assert.That(count, Is.EqualTo(1),
                $"The appsettings for test are using {count} different client secrets, instead of one");

        }

        [Test]
        public void ThatClientIdForProductionIsDifferentThanTest()
        {
            if (useOfAppsettings==SetupBaseConfigTests.AppSettingsUsage.AppSettingsIsTestWhenDev)
            {
                Assert.Inconclusive("No way to tell");
                return;
            }
            var clientIds = HelseIdWorkerConfigurasjonsForTests.DistinctBy(o => o.ClientId);
            Assert.That(helseIdWorkerConfigurasjonForProduction.ClientId, Is.Not.EqualTo(clientIds.First()), "ClientId for production is equal to clientId used for tests");
        }

        [Test]
        public void ThatClientSecretForProductionIsDifferentThanTest()
        {
            if (useOfAppsettings == SetupBaseConfigTests.AppSettingsUsage.AppSettingsIsTestWhenDev)
            {
                Assert.Inconclusive("No way to tell");
                return;
            }
            var clientSecrets = HelseIdWorkerConfigurasjonsForTests.DistinctBy(o => o.ClientSecret);
            Assert.That(helseIdWorkerConfigurasjonForProduction.ClientSecret, Is.Not.EqualTo(clientSecrets.First()), "ClientSecret for production is equal to clientISecret used for tests");
        }
    }
}
