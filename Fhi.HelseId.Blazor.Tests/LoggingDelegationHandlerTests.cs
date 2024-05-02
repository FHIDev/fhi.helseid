using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fhi.HelseId.Blazor.Tests;

public class LoggingDelegationHandlerTests
{
    [Test]
    public async Task HandlerAnonymizesUris()
    {
        var logger = new TestLogger<LoggingDelegationHandler>();
        var handler = new LoggingDelegationHandler(logger);
        handler.InnerHandler = new DummyInnerHandler();

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.TryAddWithoutValidation(CorrelationIdHandler.CorrelationIdHeaderName, "TESTCORR");
        await client.GetAsync("http://localhost/get/12345678901/?secret=value");

        Assert.That(logger.Entries.Single(),
            Contains.Substring("Information Requested HTTP GET http://localhost/get/***********/ in"));

        Assert.That(logger.Entries.Single(),
            Contains.Substring("with response 200 OK with CorrelationId TESTCORR"));

    }
}

public class TestLogger<T> : ILogger<T>
{
    public List<string> Entries = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Entries.Add(logLevel.ToString() + " " + formatter(state, exception));
    }
}