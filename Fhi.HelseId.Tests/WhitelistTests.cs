using Fhi.HelseId.Web.Services;
using Fhi.HelseId.Common.Identity;
using NUnit.Framework;
using NSubstitute;

namespace Fhi.HelseId.Tests
{
    public class WhitelistTests
    {
        Whitelist? whitelist;
        ICurrentUser? user;

        [SetUp]
        public void Init()
        {
            whitelist = new Whitelist
                {new White {Name = "Per", PidPseudonym = "1234"}, new White {Name = "Arne", PidPseudonym = "5678"}};
            user = Substitute.For<ICurrentUser>();
            user.PidPseudonym.Returns("5678");
        }


        [Test]
        public void ThatItFindsCorrectItemByUser()
        {
            Assert.Multiple(() =>
            {
                Assert.That(whitelist!.IsWhite(user!));
                Assert.That(whitelist.IsWhite(user!.PidPseudonym!));
                Assert.That(whitelist.NameOf(user), Is.EqualTo("Arne"));
            });
        }

        [Test]
        public void ThatItHandlesNonpresent()
        {
            user!.PidPseudonym.Returns("9999");
            Assert.Multiple(() =>
            {
                Assert.That(whitelist!.IsWhite(user), Is.False);
                Assert.That(whitelist.IsWhite(user.PidPseudonym!), Is.False);
                Assert.That(whitelist.NameOf(user), Is.EqualTo(""));
            });
        }

        [Test]
        public void ThatItHandlesInvalid()
        {
            Assert.That(whitelist!.IsWhite(""), Is.False);
        }
    }
}
