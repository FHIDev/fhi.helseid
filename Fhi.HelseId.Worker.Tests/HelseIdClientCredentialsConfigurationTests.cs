namespace Fhi.HelseId.Worker.Tests;

public class HelseIdClientCredentialsConfigurationTests
{
    [Test]
    public void ThatScopesCanExtractArrayToString()
    {
        var sut = new HelseIdClientCredentialsConfiguration
        {
            scopes = new[] { "one", "two", "three" }
        };
        Assert.That(sut.Scopes, Is.EqualTo("one two three"));
    }


}