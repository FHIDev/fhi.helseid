# Fhi.HelseId.Refit

This package contains code to simplify working with Blazor, Refit and HelseId. 

Blazor does not currently let you interact with the HttpContext that is needed for normal HelseId functionality in a WebApi.

Some of the problems this code solves are

- HttpContext is not available during most of the Blazor SPA lifetime
- The HelseId tokens are stored in the HttpContext which are not normally available from most of the rendering tree in the Blazor code.
- The HelseId tokens are stored in the HttpContext which are not normally available from a normal scoped or transient service when resolved with the ServiceProvider (DI).
- Refit uses HttpClientFactory, which creates transient DelegationHandlers, but they come from a singleton scope! This leads to us being unable to get the HelseId access token by the consumer of a Refit interface.
- We need to update cookies with new token data when refreshing HelseId token

This default setup will add a token handler to your Refit Interface in addition to letting you add multiple delegates if needed (f.ex. logging).

Note that HelseidRefitBuilderForBlazor is only available for server side code, and not WASM. 
We limit the usage to Server Side code to prevent the access tokens from being available in the front end.

## Usage

Include this code in your WebApi startup builder (remember to also call "builder.AddHelseIdWebAuthentication()" etc):

```cs
builder.AddHelseIdForBlazor()
    .AddRefitClient<IMyRefitClient>()

...

app.UseHelseIdForBlazor();
```

You will also need to wrap your hole App.razor code with a CascadingStates-tag:
```
<CascadingStates>
    ... all your App.razor HTML ...
</CascadingStates>
```

The code loads your configuration from IConfiguration using the section "HelseIdWebKonfigurasjon".
If you want to override which section to use you can pass the correct section to AddHelseIdForBlazor:

```cs
builder.AddHelseIdForBlazor("HelseIdWebKonfigurasjon")
    .AddRefitClient<IMyRefitClient>()
```

The default RefitSettings we are using use SystemTextJsonContentSerializer, is case insensitive and use camelCasing.
If you want to override the default RefitSettings to use, you can pass the settings to AddHelseIdForBlazor:

```cs
builder.AddHelseIdForBlazor(new RefitSettings())
    .AddRefitClient<IMyRefitClient>();
```

Note that using this builder will automatically add a middleware for logging you out, as the default HelseId-way does not work well for Blazor apps.
The URLs defaults to "/logout" and "/loggedout". You can configure the logout options in the builder options.

## Options

The default setup will add a token handler, logging handler, correlationId handler and a header-encoding handler
to your Refit Interface. In addition you can add multiple custom delegates if needed.

To add custom delegates use the AddHandler() function:

```cs
builder.AddHelseidRefitBuilder()
    .AddHandler<MyOwnLoggingDelegationHandler>();
```

You can also choose which handlers to use if you prefer not to use all the default handlers:

```cs
builder.AddHelseidRefitBuilder(builderOptions: new HelseidRefitBuilder()
    {
        UseAnonymizationLogger = true,
        HtmlEncodeFhiHeaders = true,
        UseCorrelationId = true,
        UseDefaultTokenHandler = true,
    })
    .AddRefitClient<IMyRefitClient>();
```

## Correlation Id

The Correlation Id Handler adds header propagation of the default FHI correlation id header. 
A new correlation ID will be given to each request and response that does not contain the header when invoked.
Remember to add usage of header propagation to your app startup code. It should be placed before any logging middleware:

```cs
app.UseCorrelationId();
```

## Logging

The handler "LoggingDelegationHandler" log all Refit requests with anonymized URLs. 
The logger requires dependency injection of a Microsoft.Extensions.Logging.ILogger.

The LoggingDelegationHandler will log the following messages. Uri will have all Norwegian National identity numbers replaced with start '***********), and the query parameters removed:

```
    Requested HTTP {RequestMethod} {Uri} in {Elapsed}ms with response {StatusCode} {Reason} with CorrelationId {CorrelationId}
    Requested HTTP {RequestMethod} {Uri} in {Elapsed}ms with exception {Exception} with CorrelationId {CorrelationId}
```

## Header encoding

If HtmlEncodeFhiHeaders is enabled all headers starting with the prefix "fhi-" will be automatically Html-encoded.
This is useful when using headers like "fhi-organization-name", which might contain illegal HTTP header characters.

The HTML encoding should only encode characters that are normally illegal in headers. If we did not encode them the requests would fail.

Note that headers are not automatically decoded on the receiving server! You will still have to do your own
decoding (using HttpUtility.HtmlDecode or similar), as there are no standard header-encoding rules.

Html-encoding is used over Url-encoding, since more "normal" characters, like spaces, are preserved.

## More usage

If you would like to reuse some of the code to access the HttpContext for dependency injection you can hook into the code with custom implementations of a IScopedState:

```cs
builder.AddStateHandlers().AddScopedState<UserState>();
```

An example of a implementation of UserState could be something like this.

```cs
public class UserState : IScopedState
{
    public string CorrelationId { get; set; }

    public UserState() // you can even use the constructor for normal dependency injection here!
    {
    }

    public async Task Populate(HttpContext httpContext)
    {
        var headerValue = string.Empty;

        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var values))
        {
            headerValue = values.FirstOrDefault();
        }
        else if (httpContext.Response.Headers.TryGetValue("X-Correlation-ID", out values))
        {
            headerValue = values.FirstOrDefault();
        }

        CorrelationId = string.IsNullOrEmpty(headerValue) ? Guid.NewGuid().ToString() : headerValue;
    }
}
```


## Changing default implementations

By default the code creates new HttpClients and delegates for each request, to be able to create correctly scoped delegates to apply the correct authentication token.
This is bad if your system has a lot of users as it may lead to socket exhaustion.

You can change the default implementation by providing your own IScopedHttpClientFactory
```cs
services.AddSingleton<IScopedHttpClientFactory>(new MyOwnScopedHttpClientFactory());
```

If you are able to create a better implementation please consider making a pull request to change our ScopedHttpClientFactory.

Note that the ScopedHttpClientFactory creates the handler in the opposite direction than IHttpClientFactory. This is because we want to end
up with applying authorization and correlation id before the user may add logging delegates, and finally having the default httpclienthandler 
as the innermost handler.

You can also change the default HttpClientHandler builder if you please. Note that if you do you might also want to change if the handlers should be disposed or not after HttpClients are disposed (defaults to true):

```cs
builder.AddHelseIdForBlazor()
    .SetHttpClientHandlerBuilder(name => new HttpClientHandler())
    .DisposeHandleres(true)
```