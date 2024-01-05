# Fhi.HelseId.Refit

This package contains code to simplify working with Blazor, Refit and HelseId. 

Blazor does not currently let you interact with the HttpContext that is needed for normal HelseId funtionallity in a WebApi.

Some of the problems this code solves are

- HttpContext is not availible during most of the Blazor SPA lifetime
- The HelseId tokens are stored in the HttpContext which are not normally availible from most of the rendering tree in the Blazor code.
- The HelseId tokens are stored in the HttpContext which are not normally availible from a normal scoped or transient service when resolved with the ServiceProvider (DI).
- Refit uses HttpClientFactory, which creates transient DelegationHandlers, but they come from a singleton scope! This leads to us being unable to get the HelseId access token by the consumer of a Refit interface.
- We need to update cookies with new token data when refreshing HelseId token

This default setup will add a token handler to your Refit Interface in addition to letting you add multiple delegates if needed (f.ex. logging).

## Usage

Include this code in your WebApi startup builder (remember to also call "builder.AddHelseIdWebAuthentication()" etc):

```
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



If you want to add additional loggers add them before "AddRefitClient": 

```
builder.AddHelseIdForBlazor()
    .AddHandler<MyLoggingDelegationHandler>()
    .AddRefitClient<IMyRefitClient>()
```
The code loads your configuration from IConfiguration using the section "HelseIdWebKonfigurasjon".
If you want to override which section to use you can pass the correct section to AddHelseIdForBlazor:

```
builder.AddHelseIdForBlazor("HelseIdWebKonfigurasjon")
    .AddRefitClient<IMyRefitClient>()
```

The default RefitSettings we are using use SystemTextJsonContentSerializer, is case insensitive and use camelCasing.
If you want to override the default RefitSettings to use you can pass the settings to AddHelseIdForBlazor:

```
builder.AddHelseIdForBlazor(new RefitSettings())
    .AddRefitClient<IMyRefitClient>();
```


Note that using this builder will automatically add a middleware for logging you out, as the default HelseId-way does not work well for Blazor apps.
The URLs defaults to "/logout" and "/loggedout". You can configure the logout options by calling

```
builder.AddHelseIdForBlazor()
    .ConfigureLogout(....);
```


## Adding Correlation Id to all requests

Use "AddCorrelationId()" to add header propagation of the default FHI correlation id header. 

```
builder.AddHelseIdForBlazor()
    .AddCorrelationId()
    .AddRefitClient<IMyRefitClient>();
```

A new correlation ID will be given to each request and response that does not contain the header when invoked.

## More usage

If you would like to reuse some of the code to access the HttpContext for dependency injection you can hook into the code with custom implementations of a IScopedState:

```
builder.AddStateHandlers().AddScopedState<UserState>();
```

An example of a implementation of UserState could be something like this.

```
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
```
services.AddSingleton<IScopedHttpClientFactory>(new MyOwnScopedHttpClientFactory());
```

If you are able to create a better implementation please consider making a pull request to change our ScopedHttpClientFactory.

Note that the ScopedHttpClientFactory creates the handler in the opposite direction than IHttpClientFactory. This is because we want to end
up with applying authorization and correlation id before the user may add logging delegates, and finally having the default httpclienthandler 
as the innermost handler.

You can also change the default HttpClientHandler builder if you please. Note that if you do you might also want to change if the handlers should be disposed or not after HttpClients are disposed (defaults to true):

```
builder.AddHelseIdForBlazor()
    .SetHttpClientHandlerBuilder(name => new HttpClientHandler())
    .DisposeHandleres(true)
```