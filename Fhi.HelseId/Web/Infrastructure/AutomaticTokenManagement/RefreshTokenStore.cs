using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Fhi.HelseId.Web.Services;
using IdentityModel.Client;

namespace Fhi.HelseId.Web.Infrastructure.AutomaticTokenManagement;

public interface IRefreshTokenStore
{
    List<RefreshToken> RefreshTokens { get; set; }
    RefreshToken? GetLatestToken(ICurrentUser user);
    void Add(string previousToken, TokenResponse? tokenResponse, ICurrentUser user, [CallerMemberName] string source = "", [CallerLineNumber] int lineNumber = 0);
    void Dump();
    void AddIfNotExist(string previousToken, TokenResponse? tokenResponse, ICurrentUser user,[CallerMemberName] string source = "", [CallerLineNumber] int lineNumber = 0);
    bool Exist(string token, ICurrentUser user);
    bool IsLatest(string refreshTokenValue, ICurrentUser user);
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

    public void Add(string previousToken, TokenResponse? tokenResponse, ICurrentUser user, [CallerMemberName] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
        var dtExpiresNew = DateTimeOffset.Now.AddSeconds(tokenResponse?.ExpiresIn ?? 0);
        var newRefreshToken = new RefreshToken
        {
            PreviousToken = previousToken,
            NewToken = tokenResponse?.RefreshToken ?? "",
            Acquired = DateTimeOffset.Now,
            SequenceNumber = currentSequenceNumber++,
            AccessToken = tokenResponse?.AccessToken ?? "",
            Source = $"{source},l.{lineNumber}",
            ExpireAt = dtExpiresNew,
            PidPseudonym = user?.PidPseudonym??"",
        };
        if (user?.Name != null)
        {
            newRefreshToken.NameObfuscated = user.Name is { Length: > 5 } ? user.Name[..5] : user.Name;
        }
        newRefreshToken.AccessTokenHash = newRefreshToken.AccessToken.GetHashCode();
        RefreshTokens.Add(newRefreshToken);

    }

    public void AddIfNotExist(string previousToken, TokenResponse? tokenResponse, ICurrentUser user, [CallerMemberName] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (!RefreshTokens.Exists(o => o.PreviousToken == previousToken && o.PidPseudonym==user.PidPseudonym))
        {
            Add(previousToken, tokenResponse, user, source, lineNumber);
        }
    }

    public bool Exist(string token,ICurrentUser user) => RefreshTokens.Exists(o => o.CurrentToken == token && o.PidPseudonym==user.PidPseudonym);
    public bool IsLatest(string refreshTokenValue,ICurrentUser user)
    {
        if (!RefreshTokens.Any())
            return true;
        var latest = GetLatestToken(user);
        return latest?.CurrentToken == refreshTokenValue;
    }

    public RefreshToken? GetLatestToken(ICurrentUser user) => RefreshTokens.Where(o=>o.PidPseudonym==user.PidPseudonym).MaxBy(o => o.SequenceNumber);

    public void Dump()
    {
        logger.LogTrace("{class}.{method}", nameof(RefreshTokenStore), nameof(Dump));
        foreach (var token in RefreshTokens)
        {
            logger.LogTrace("{tokens}", token.ToString());
        }
    }
}

public class RefreshToken
{
    public string PreviousToken { get; set; } = "";

    public string NameObfuscated { get; set; } = "";

    public string NewToken { get; set; } = "";

    public int AccessTokenHash { get; set; }
    public DateTimeOffset Acquired { get; set; }

    public int SequenceNumber { get; set; }
    public string Source { get; set; } = "";

    public string CurrentToken => string.IsNullOrEmpty(NewToken) ? PreviousToken : NewToken;
    public DateTimeOffset ExpireAt { get; set; }
    public string AccessToken { get; set; } = "";
    public string PidPseudonym { get; set; } = "";

    public override string ToString()
    {
        return $"{Acquired:HH:mm:ss:fff} No: {SequenceNumber} : User: {NameObfuscated} Prev: {PreviousToken}  New: {NewToken}, Source : {Source}";
    }

}