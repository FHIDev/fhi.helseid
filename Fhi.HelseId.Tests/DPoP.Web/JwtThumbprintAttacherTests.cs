using Fhi.HelseId.Web.DPoP;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests.DPoP.Web;

internal class JwtThumbprintAttacherTests
{
    private ProofKeyConfiguration _keyConfiguration;
    private JwtThumbprintAttacher _thumbprintAttacher;
    private RedirectContext _redirectContext;

    [SetUp]
    public void SetUp()
    {
        var proofKey = Substitute.For<JsonWebKey>();
        proofKey.ComputeJwkThumbprint().Returns([1, 2, 3, 4, 5]);
        _keyConfiguration = new ProofKeyConfiguration(proofKey);
        _thumbprintAttacher = new JwtThumbprintAttacher(_keyConfiguration);
        var authScheme = new AuthenticationScheme("test", "test", typeof(IAuthenticationHandler));
        _redirectContext = new RedirectContext(
            Substitute.For<HttpContext>(),
            authScheme, new OpenIdConnectOptions(),
            new AuthenticationProperties());
        _redirectContext.ProtocolMessage = new OpenIdConnectMessage();
    }

    [Test]
    public void AttachThumbprint_AddsJwkThumbprintToProtocolMessage()
    {
        // Act
        _thumbprintAttacher.AttachThumbprint(_redirectContext);

        // Assert
        var expectedThumbprint = Base64UrlEncoder.Encode([1, 2, 3, 4, 5]);
        Assert.That(_redirectContext.ProtocolMessage.Parameters["dpop_jkt"], Is.EqualTo(expectedThumbprint));
    }

    [Test]
    public void AttachThumbprint_SetsContextKeyInProperties()
    {
        // Act
        _thumbprintAttacher.AttachThumbprint(_redirectContext);

        // Assert
        Assert.That(_redirectContext.Properties.Items[DPoPContext.ContextKey], Is.EqualTo("true"));
    }
}
