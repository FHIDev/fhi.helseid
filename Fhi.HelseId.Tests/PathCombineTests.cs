using System;
using NUnit.Framework;

namespace Fhi.HelseId.Tests;

internal class PathCombineTests
{
    [TestCase("https://helseid-test.nhn.no", "/connect/token", "https://helseid-test.nhn.no/connect/token")]
    [TestCase("https://helseid-test.nhn.no/", "/connect/token", "https://helseid-test.nhn.no/connect/token")]
    [TestCase("https://helseid-test.nhn.no/", "connect/token", "https://helseid-test.nhn.no/connect/token")]
    [TestCase("https://helseid-test.nhn.no", "connect/token", "https://helseid-test.nhn.no/connect/token")]
    public void CombineTests(string path1, string path2, string expected)
    {
        var uri = new Uri(new Uri(path1), path2).AbsoluteUri;
        Assert.That(uri, Is.EqualTo(expected));
    }
}