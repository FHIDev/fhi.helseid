using Fhi.HelseId.Api;
using Fhi.HelseId.Api.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder();

var configAuth = new HelseIdApiKonfigurasjon
{
    Authority = "https://helseid-sts.test.nhn.no/",
    ApiName = "fhi:helseid.testing.api",
    ApiScope = "fhi:helseid.testing.api/all",
    AuthUse = true,
    UseHttps = true,
    RequireContextIdentity = true,
};

builder.Services.AddHelseIdApiAuthentication(configAuth);
builder.Services.AddHelseIdAuthorizationControllers(configAuth);

var app = builder.Build();

app.MapGet("/api/test", [Authorize] () => "Hello world!");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
