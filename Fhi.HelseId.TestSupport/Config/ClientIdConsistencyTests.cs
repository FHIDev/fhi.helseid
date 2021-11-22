using System.Collections.Generic;
using System.Linq;
using Fhi.HelseId.Worker;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using MoreLinq;

namespace Fhi.HelseId.TestSupport.Config
{
    public abstract class ClientIdConsistencyTests
    {
        private List<HelseIdWorkerKonfigurasjon> HelseIdWorkerConfigurasjonsForTests { get; } =
            new List<HelseIdWorkerKonfigurasjon>();


        private readonly HelseIdWorkerKonfigurasjon helseIdWorkerConfigurasjonForProduction;

        /// <summary>
        /// Add the different types of appsettings you have for tests,  e.g. appsettings.test.json => test
        /// </summary>
        /// <param name="types"></param>
        protected ClientIdConsistencyTests(List<string> types)
        {
            var fileNamesForTests = types.Select(o => $"appsettings.{o}.json");

            foreach (var fileName in fileNamesForTests)
            {
                var workerConfig = new HelseIdWorkerClientIds(fileName).HelseIdWorkerKonfigurasjonUnderTest;
                HelseIdWorkerConfigurasjonsForTests.Add(workerConfig);
            }

            helseIdWorkerConfigurasjonForProduction = new HelseIdWorkerClientIds("appsettings.json").HelseIdWorkerKonfigurasjonUnderTest;
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
            var clientIds = HelseIdWorkerConfigurasjonsForTests.DistinctBy(o => o.ClientId);
            Assert.That(helseIdWorkerConfigurasjonForProduction.ClientId, Is.Not.EqualTo(clientIds.First()), "ClientId for production is equal to clientId used for tests");
        }

        [Test]
        public void ThatClientSecretForProductionIsDifferentThanTest()
        {
            var clientSecrets = HelseIdWorkerConfigurasjonsForTests.DistinctBy(o => o.ClientSecret);
            Assert.That(helseIdWorkerConfigurasjonForProduction.ClientSecret, Is.Not.EqualTo(clientSecrets.First()), "ClientSecret for production is equal to clientISecret used for tests");
        }
    }

    public class HelseIdWorkerClientIds : SetupBaseConfigTests
    {
        public HelseIdWorkerKonfigurasjon HelseIdWorkerKonfigurasjonUnderTest { get; set; }
        public HelseIdWorkerClientIds(string configFile) : base(configFile)
        {
            HelseIdWorkerKonfigurasjonUnderTest = Config.GetSection(nameof(HelseIdWorkerKonfigurasjon))
                .Get<HelseIdWorkerKonfigurasjon>();
        }

        protected override void Guard()
        {
            // Does nothing here
        }
    }
}
