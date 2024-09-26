﻿using Fhi.HelseId.Common.DPoP;
using Fhi.HelseId.Web.DPoP;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Fhi.HelseId.Web;

public interface IAuthorizationHeaderSetter
{
    Task SetAuthorizationHeader(HttpRequestMessage request, string token);
}

public class BearerAuthorizationHeaderSetter : IAuthorizationHeaderSetter
{
    public Task SetAuthorizationHeader(HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme.Bearer, token);
        return Task.CompletedTask;
    }
}

public class DPoPAuthorizationHeaderSetter(IDPoPTokenCreator dPoPTokenCreator) : IAuthorizationHeaderSetter
{
    public async Task SetAuthorizationHeader(HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme.DPoP, token);

        var athValue = AccessTokenHash.Sha256(token);
        var proof = await dPoPTokenCreator.CreateSignedToken(request.Method, request.RequestUri!.ToString(), ath: athValue);
        request.Headers.Add(DPoPHttpHeaders.ProofHeaderName, proof);
    }
}