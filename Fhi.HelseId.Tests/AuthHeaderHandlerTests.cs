using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fhi.HelseId.Common;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.Handlers;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;

namespace Fhi.HelseId.Tests;
public class AuthHeaderHandlerTests
{
    [Test]
    public async Task HandlerAddsAuthToken()
    {
        var authToken = Guid.NewGuid().ToString();

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = Substitute.For<HttpContext>();

        var services = new ServiceCollection();

        var logger = Substitute.For<ILoggerFactory>();
        services.AddSingleton(logger);

        //services.AddAuthentication("NoAuthentication")
        //        .AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>("NoAuthentication", null);

        services.AddHttpContextAccessor();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "TODO_your_issuer",
                    ValidAudience = "TODO_your_audience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TODO_your_secret_key_here")),
                    ClockSkew = TimeSpan.Zero
                };
            });

        context.RequestServices.Returns(services.BuildServiceProvider());
        httpContextAccessor.HttpContext.Returns(context);

        var handler = new AuthHeaderHandler(
            httpContextAccessor,
            NullLogger<AuthHeaderHandler>.Instance,
            Substitute.For<ICurrentUser>(),
            Options.Create(new HelseIdWebKonfigurasjon()),
            new BearerAuthorizationHeaderSetter());

        handler.InnerHandler = new DummyInnerHandler();

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        var response = await client.SendAsync(request);

        var token = await response.Content.ReadAsStringAsync();

        // the dummy client returns the authorization header
        Assert.That(token, Is.EqualTo("Bearer " + authToken));
    }

    [Test]
    public async Task HandlerDoesNotGetTokenWhenAnonymous()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = Substitute.For<HttpContext>();
        var services = new ServiceCollection();
        context.RequestServices.Returns(services.BuildServiceProvider());
        httpContextAccessor.HttpContext.Returns(context);

        var handler = new AuthHeaderHandler(
            httpContextAccessor,
            NullLogger<AuthHeaderHandler>.Instance,
            Substitute.For<ICurrentUser>(),
            Options.Create(new HelseIdWebKonfigurasjon()),
            new BearerAuthorizationHeaderSetter());

        handler.InnerHandler = new DummyInnerHandler();

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        request.Options.TryAdd("Anonymous", "Anonymous");
        var response = await client.SendAsync(request);

        var token = await response.Content.ReadAsStringAsync();

        Assert.That(token, Is.EqualTo(""));
    }
}

public class DummyInnerHandler : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Get any auth header, and use its value as the content of the request
        var auth = request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value?.FirstOrDefault() ?? "";
        var response = new HttpResponseMessage() { Content = new StringContent(auth) };
        return Task.FromResult(response);
    }
}