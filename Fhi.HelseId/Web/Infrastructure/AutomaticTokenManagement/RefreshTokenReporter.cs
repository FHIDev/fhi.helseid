using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;

public interface IRefreshTokenReporter
{
    List<RefreshToken> RefreshTokens { get; set; }
    void Add(string previousToken, string newToken, string accessToken,string source, [CallerLineNumber] int lineNumber = 0);
    void Dump();
    void AddIfNotExist(string previousToken, string newToken, string accessToken, string source, [CallerLineNumber] int lineNumber = 0);
}

public class RefreshTokenReporter : IRefreshTokenReporter
{
    private readonly ILogger<RefreshTokenReporter> logger;

    public RefreshTokenReporter(ILogger<RefreshTokenReporter> logger)
    {
        this.logger = logger;
    }

    static int currentSequenceNumber = 0;
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    public void Add(string previousToken, string newToken, string accessToken,string source, [CallerLineNumber] int lineNumber = 0)
    {
        RefreshTokens.Add(new RefreshToken
        {
            PreviousToken = previousToken, 
            NewToken=newToken, 
            Acquired = DateTimeOffset.Now, 
            SequenceNumber = currentSequenceNumber++,
            AccessTokenHash = accessToken.GetHashCode(),
            Source= $"{source},l.{lineNumber}"
        });
    }

    public void AddIfNotExist(string previousToken, string newToken, string accessToken, string source, [CallerLineNumber] int lineNumber = 0)
    {
        if (!RefreshTokens.Exists(o => o.PreviousToken == previousToken))
        {
            Add(previousToken, newToken, accessToken, source);
        }
    }


    public void Dump()
    {
        logger.LogTrace("{class}.{method}", nameof(RefreshTokenReporter), nameof(Dump));
        foreach (var token in RefreshTokens)
        {
            logger.LogTrace("{tokens}",token.ToString());
        }
    }   
}

public class RefreshToken
{
    public string PreviousToken { get; set; } = "";

    public string NewToken { get; set; } = "";

    public int AccessTokenHash { get; set; }
    public DateTimeOffset Acquired { get; set; } 

    public int SequenceNumber { get; set; }
    public string Source { get; set; } = "";

    public override string ToString()
    {
        return $"{Acquired:HH:mm:ss:fff} No: {SequenceNumber} : Prev: {PreviousToken}  New: {NewToken}, Source : {Source}";
    }
    
}