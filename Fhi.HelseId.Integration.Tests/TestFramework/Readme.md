# Testing guideline

## General guideline

- Treat tests as production code
- It should be understandable what the tests do and follow AAA(Arange, Act, Assert) pattern
- Test names should follow the GWT(Given, When, Then), but without including GWT

```
    [Test]
    public async Task CallTestApiEnpointPerson_ConfiguredXXisTrue_ReturnYYinResponse()
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

        // ACT
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
        public async Task SampleSetup_WithOverrideOpenIdConnectConfigurationInTests()
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
                //Mock services   
            });

            // Create HttpClient
            var client = appFactory.CreateClient();
            // Call an endpoint
            var response = await client.GetAsync("xxx");
      }

```


#### Sample of overriding OIDC options

```
    [Test]
        public async Task SampleSetup_WithOverrideOpenIdConnectConfigurationInTests()
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


#### Sample of fake authentication


```
[Test]
public async Task Overriding_CookieAuth()
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
