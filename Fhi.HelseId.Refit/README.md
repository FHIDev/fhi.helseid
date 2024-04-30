# Fhi.HelseId.Refit

This package contains code to simplify working with Refit and HelseId. 

## Usage

Include this code in your WebApi startup builder. 
Don't forget to call UseCorrelationId() after building your application if you are using Correlation Id:

```
builder.AddHelseidRefitBuilder()
    .AddRefitClient<IMyRefitClient>();

...

app.UseCorrelationId();
```

The code loads your configuration from IConfiguration using the section "HelseIdWebKonfigurasjon".
If you want to override which section to use you can pass the correct section to AddHelseidRefitBuilder:

```
builder.AddHelseidRefitBuilder("CustomHelseIdWebKonfigurasjon")
    .AddRefitClient<IMyRefitClient>();
```

The default RefitSettings we are using use SystemTextJsonContentSerializer, is case insensitive and use camelCasing.
If you want to override the default RefitSettings to use you can pass the settings to AddHelseidRefitBuilder:

```
builder.AddHelseidRefitBuilder(new RefitSettings())
    .AddRefitClient<IMyRefitClient>();
```


## Options

This default setup will add a token handler, logging handler, correlationId handler and an header-encoding handler
to your Refit Interface. In addition you can add multiple custom delegates if needed.

To add custom delegates use the AddHandler() function:

```
builder.AddHelseidRefitBuilder()
    .AddHandler<MyOwnLoggingDelegationHandler>();
```

You can also choose which handlers to use if you prefer not to use all the default handlers:
```
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

```
app.UseCorrelationId();
```

## Logging

The handler "LoggingDelegationHandler" log all Refit requests with anonymized URLs. 
The logger requires dependency injection of a Microsoft.Extensions.Logging.ILogger.

The LoggingDelegationHandler will log the following messages. Uri will have all Nowrwegian National identity numbers replaced with start '***********), and the query parameters removed:

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