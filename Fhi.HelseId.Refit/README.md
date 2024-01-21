# Fhi.HelseId.Refit

This package contains code to simplify working with Refit and HelseId. 

This default setup will add a token handler to your Refit Interface in addition to letting you add multiple delegates if needed (f.ex. logging).

## Usage

Include thhis code in your WebApi startup builder:

```
builder.AddHelseidRefitBuilder()
    .AddRefitClient<IMyRefitClient>();
```

If you want to add additional loggers add them before "AddRefitClient": 

```
builder.AddHelseidRefitBuilder()
    .AddHandler<MyLoggingDelegationHandler>()
    .AddRefitClient<IMyRefitClient>();
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

## Adding Correlation Id to all requests

Use "AddCorrelationId()" to add header propagation of the default FHI correlation id header. 

```
builder.AddHelseidRefitBuilder()
    .AddCorrelationId()
    .AddRefitClient<IMyRefitClient>();
```

A new correlation ID will be given to each request and response that does not contain the header when invoked.
Remember to add usage of header propagation to your app startup code:

```
app.UseHeaderPropagation();
```
