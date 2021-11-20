using NUnit.Framework;

namespace Fhi.HelseId.TestSupport
{
    /// <summary>
    /// Derive a class in your own concrete test project, and the tests here will be automatically run
    /// </summary>
    public abstract class ConfigurationTestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ThatAuthorityIsCorrect()
        {
            Assert.Pass();
        }
    }

  
}