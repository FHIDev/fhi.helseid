# Testing guideline

## General guideline

- Treat tests as production code
- It should be understandable what the tests do and follow AAA(Arange, Act, Assert) pattern
- Test names should follow the [GWT(Given, When, Then)](https://martinfowler.com/bliki/GivenWhenThen.html), but without including GWT 

```
    [Test]
    public async Task ConfiguredXXisTrue_CallTestApiEnpointPerson_ReturnYYinResponse()
    {
        // Arrange
        var appsettingsConfig = new Dictionary<string, string?>
        {
            "XX", "true" },
            
        };
        var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
        // Use testframeork application factory to override configurations and services
        var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
        {
            ..   
        });

        // Act
        var client = appFactory.CreateClient();
        var response = await client.GetAsync("api/person");

        //Assert
         var content = await response.Content.Content.ReadAsStringAsync();
         Assert.That(content, Is.EqualTo("YY"));

    }

```



### Using test framework

The Testframework uses [MS integrationtest framework](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-9.0) to interfare with the request response pipeline. 
This allows to test different configurations and to simulate different usages by mocking out parts of the services. 

```
 [Test]
        public async Task Given_When_Then()
        {
            //Configure appsettings
            var appsettingsConfig = new Dictionary<string, string?>
            {
               ...
            };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
            
            // Use testframeork application factory to override configurations and services
            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                //Add overriding of services here   
            });

            
            var client = appFactory.CreateClient();
            var response = await client.GetAsync("xxx");
      }

```


#### Sample of overriding OIDC options

```
    [Test]
        public async Task Given_When_Then()
        {
            var appsettingsConfig = new Dictionary<string, string?>
            {
               ...
            };
            var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
            var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
            {
                services.AddHelseIdWebAuthentication(testConfiguration)
               .Build();

                //Create a new IConfigureNamedOptions to override OIDC authentication scheme options
                services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureOidcOptions>();
            });


            var client = appFactory.CreateClient();
            var response = await client.GetAsync("/api/test");
      }



        /// <summary>
        /// Sample on overidden configuration
        /// </summary>
        internal class ConfigureOidcOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            public void Configure(string? name, OpenIdConnectOptions options)
            {
                Configure(options);
            }

            /// <summary>
            /// sample on overridden configuration. 
            /// Overriding evenst for illustration
            /// </summary>
            /// <param name="options"></param>
            public void Configure(OpenIdConnectOptions options)
            {
                options.CallbackPath = "https://customized_redirect_uri";
                options.Events.OnAuthorizationCodeReceived = ctx =>
                {
                    return Task.CompletedTask;
                };
            }
        }

```

#### Sample of fake authentication using test authentication scheme


```
    [Test]
    public async Task Given_When_Then()
    {
        //// Create configuration of HelseId
        var appsettingsConfig = new Dictionary<string, string?>
        {
            ...
        };
        var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();
        //// Generate access_token and id_token from TTT based on the application configuration
        var (AccessToken, IdToken) = await CreateAccessAndIdToken(config.ClientId, config.AllScopes.ToList(), securityLevel: "2");

        var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
        {
            //// Add fake authentication where User contains claims from access_token and id_token
            services.AddFakeTestAuthenticationScheme(AccessToken, IdToken);
            services.AddHelseIdWebAuthentication(testConfiguration)
            .Build();
        });


        var testClient = appFactory.CreateClient();
        var response = await testClient.GetAsync("/api/test");
    
        //Assert...
    }

    private static async Task<(string AccessToken, string IdToken)> CreateAccessAndIdToken(
    string clientId,
    IEnumerable<string> scopes,
    string audience = "fhi:api",
    string? securityLevel = "4")
    {
        var accessToken = await TTTTokenService.GetHelseIdToken(TTTTokenRequests.DefaultAccessToken(scopes.ToList(), audience));
        var idToken = await TTTTokenService.GetHelseIdToken(TTTTokenRequests.IdToken(clientId, scopes.ToList(), securityLevel: securityLevel));

        return (accessToken, idToken);
    }

```


#### Sample of fake authentication using cookie


```
[Test]
public async Task Given_When_Then()
{
    var appsettingsConfig = new Dictionary<string, string?>
    {
        ...
    };
    var testConfiguration = appsettingsConfig.BuildInMemoryConfiguration();

    var appFactory = new TestWebApplicationFactory(testConfiguration, services =>
    {
        services.AddHelseIdWebAuthentication(testConfiguration)
        .Build();

        //Simulate cookie
        services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();

    });


    var testClient = appFactory.CreateClient();
    var response = await testClient.GetAsync("/api/test");
}
```
