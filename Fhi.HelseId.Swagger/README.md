# Fhi.HelseId.Swagger

This package provides infrastructure so that developers can easily use Swagger with HelseID and generate tokens within their browsers. This functionality is limited in Swagger out of the box due to the fact it does not yet support JWT Bearer client assertions when creating the token. This package adds a test token endpoint that will act as a proxy between Swagger and authority in order to create a token request towards the authority using a JWT Bearer client assertion.

The endpoint that is provided in this package is only intended to be used in development context and not in a production environment.

# Getting started

The example bellow is intended for the usage of an API that requires a token using the authorization code flow.

## appsettings.json

You will need a separate client in HelseID selvbetjening for the Swagger client. After creating the client, add the HelseID configuration to appsettings under a `SwaggerHelseIdConfiguration` property and add the `TokenEndpoint` property as follows:

```json
  "SwaggerHelseIdConfiguration": {
    "TokenEndpoint": "https://localhost:7113/.swagger-dev/token",
    "clientName": "FOLKEHELSEINSTITUTTET -  Fhi.Sysvak.Epj.Testklient.dev (Test)",
    "authority": "https://helseid-sts.test.nhn.no",
    "clientId": "fcffea32-8f2d-4efc-a25a-b668a788d4f1",
    "grantTypes": [
      "authorization_code"
    ],
    "scopes": [
      "openid",
      "helseid://scopes/hpr/hpr_number",
      "fhi:sysvak.epj.dev/api"
    ],
    "redirectUris": [
      "https://localhost:44380/swagger/oauth2-redirect.html"
    ],
    "postLogoutRedirectUris": [],
    "secretType": "private_key_jwt:RsaPrivateKeyJwtSecret",
    "rsaPrivateKey": "\u003CRSAKeyValue\u003E\u003...",
    "rsaKeySizeBits": 4096,
    "privateJwk": "{\u0022d\u0022:\u0022Qo5q2Fo...}",
    "pkceRequired": true
  }
```

The `TokenEndpoint` will point towards the test token endpoint that Swagger will use to obtain an authorization token.

## Program.cs

Add required services and configuration:

```csharp
if (builder.Environment.IsDevelopment())
{
    var swaggerAuthConfig = builder.Configuration.GetSection(nameof(SwaggerHelseIdConfiguration)).Get<SwaggerHelseIdConfiguration>()!;
    builder.AddHelseIdTestTokens(swaggerAuthConfig);
}
```

Ensure that `AddSwaggerGen` is configured to use the auth token:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri($"{swaggerAuthConfig.Authority}/connect/authorize"),
                TokenUrl = new Uri(swaggerAuthConfig.TokenEndpoint),
                Scopes = swaggerAuthConfig.Scopes.ToDictionary(k => k, v => string.Empty)
            }
        },
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
    /// ...
});
```

and add the test token end point together with configuration of how Swagger UI will build the token requests:
```csharp
if (app.Environment.IsDevelopment())
{
    var swaggerAuthConfig = app.Services.GetRequiredService<SwaggerHelseIdConfiguration>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthAppName(swaggerAuthConfig.ClientName);
        options.OAuthClientId(swaggerAuthConfig.ClientId);

        options.OAuthScopeSeparator(" ");
        options.OAuthUsePkce();
        options.OAuthScopes(swaggerAuthConfig.Scopes);
    });
    app.UseHelseIdTestTokenEndpoint();
}
```

