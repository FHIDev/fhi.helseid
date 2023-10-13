using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;

public interface IRefreshTokenStore
{
    List<RefreshToken> RefreshTokens { get; set; }
    RefreshToken? GetLatestToken { get; }
    void Add(string previousToken, string newToken, DateTimeOffset expireAt, string accessToken,string source, [CallerLineNumber] int lineNumber = 0);
    void Dump();
    void AddIfNotExist(string previousToken, string newToken, DateTimeOffset expireAt,string accessToken, string source, [CallerLineNumber] int lineNumber = 0);
    bool Exist(string token);
    bool IsLatest(string refreshTokenValue);
}

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly ILogger<RefreshTokenStore> logger;

    public RefreshTokenStore(ILogger<RefreshTokenStore> logger)
    {
        this.logger = logger;
    }

    static int currentSequenceNumber;
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    public void Add(string previousToken, string newToken, DateTimeOffset expireAt, string accessToken,string source, [CallerLineNumber] int lineNumber = 0)
    {
        RefreshTokens.Add(new RefreshToken
        {
            PreviousToken = previousToken, 
            NewToken=newToken, 
            Acquired = DateTimeOffset.Now, 
            SequenceNumber = currentSequenceNumber++,
            AccessToken = accessToken,
            AccessTokenHash = accessToken.GetHashCode(),
            Source= $"{source},l.{lineNumber}",
            ExpireAt = expireAt
        });
    }

    public void AddIfNotExist(string previousToken, string newToken, DateTimeOffset expireAt, string accessToken, string source, [CallerLineNumber] int lineNumber = 0)
    {
        if (!RefreshTokens.Exists(o => o.PreviousToken == previousToken))
        {
            Add(previousToken, newToken, expireAt,accessToken, source);
        }
    }

    public bool Exist(string token) => RefreshTokens.Exists(o=>o.CurrentToken == token);
    public bool IsLatest(string refreshTokenValue)
    {
        if (!RefreshTokens.Any())
            return true;
        var latest = GetLatestToken;
        return latest?.CurrentToken == refreshTokenValue;
    }

    public RefreshToken? GetLatestToken => RefreshTokens.MaxBy(o=>o.SequenceNumber);

    public void Dump()
    {
        logger.LogTrace("{class}.{method}", nameof(RefreshTokenStore), nameof(Dump));
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

    public DateTimeOffset Expire => Acquired.AddMinutes(60);

    public int SequenceNumber { get; set; }
    public string Source { get; set; } = "";

    public string CurrentToken => string.IsNullOrEmpty(NewToken) ? PreviousToken : NewToken;
    public DateTimeOffset ExpireAt { get; set; }
    public string AccessToken { get; set; } = "";

    public override string ToString()
    {
        return $"{Acquired:HH:mm:ss:fff} No: {SequenceNumber} : Prev: {PreviousToken}  New: {NewToken}, Source : {Source}";
    }
    
}