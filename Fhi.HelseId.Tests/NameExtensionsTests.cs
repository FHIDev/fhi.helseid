using NUnit.Framework;
using Fhi.HelseId.Web.ExtensionMethods;

namespace Fhi.HelseId.Tests
{
    public class NameExtensionsTests
    {
        [TestCase("Ax", "Ax*******")]
        [TestCase("Arne Person", "Arn*******")]
        [TestCase(null, "(null)")]
        [TestCase("", "*******")]
        public void ObfuscateTest(string? name, string expected)
        {
            var res = name.ObfuscateName();
            Assert.That(res, Is.EqualTo(expected));
        }
    }
}
