using System.Security.Claims;
using System.Threading.Tasks;
using Fhi.HelseId.Api;
using Fhi.HelseId.Api.Handlers;
using Fhi.HelseId.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests
{
    internal class AuthHandlerTests
    {
        [TestCase("vg:paper/read", false)]
        [TestCase("fhi:grunndata.personoppslagapi/whatever", false)]
        [TestCase("fhi:grunndata.personoppslagapi/all, fhi:grunndata.personoppslagapi/grunndata", true)]
        [TestCase("fhi:grunndata.personoppslagapi/grunndata, fhi:grunndata.personoppslagapi/sysvak",true)]
        public async Task AuthHandlerTest(string configScopes,bool expected)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("scope", "fhi:grunndata.personoppslagapi/grunndata"),
                new Claim("scope", "fhi:grunndata.personoppslagapi/all")
            }));
            var helseIdConfig = Substitute.For<IHelseIdApiKonfigurasjon>();
            helseIdConfig.ApiScope.Returns(configScopes);
            helseIdConfig.ApiName.Returns("fhi:grunndata.personoppslagapi");
            var logger = Substitute.For<ILogger<ApiMultiScopeHandler>>();
            var sut = new ApiMultiScopeHandler(helseIdConfig, logger);
            var context = new AuthorizationHandlerContext(new[] { new SecurityLevelOrApiRequirement() }, user, new object());
            await sut.HandleAsync(context);
            Assert.That(context.HasSucceeded, Is.EqualTo(expected));
        }
    }
}
