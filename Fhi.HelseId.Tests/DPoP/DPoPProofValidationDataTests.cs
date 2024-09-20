using Fhi.HelseId.Common.DPoP;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fhi.HelseId.Tests.DPoP;

public class DPoPProofValidationDataTests
{
    private IReplayCache _replayCache;
    private DPoPProofValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _replayCache = Substitute.For<IReplayCache>();
        _validator = new DPoPProofValidator(_replayCache);
    }

    [Test]
    public async Task Validate_ReturnsSuccess_WhenValidationPasses()
    {
        // Arrange
        var validationData = CreateValidDPoPProofValidationData();
        _replayCache.ExistsAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(false));

        // Act
        var result = await _validator.Validate(validationData);

        // Assert
        Assert.That(result.IsError, Is.False);
        Assert.That(result, Is.EqualTo(ValidationResult.Success()));
    }

    private DPoPProofValidationData CreateValidDPoPProofValidationData()
    {
        var request = Substitute.For<HttpRequest>();
        request.Scheme = "https";
        request.Host = new HostString("example.com");
        request.PathBase = "/api";
        request.Path = "/resource";
        request.Method = "GET";

        var accessToken = "validAccessToken";
        var proofToken = "validProofToken";
        var cnfClaimValue = "{\"jkt\":\"validJktValue\"}";

        return new DPoPProofValidationData(request, proofToken, accessToken, cnfClaimValue);
    }
}
