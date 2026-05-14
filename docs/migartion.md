# Migration Guide

This guide is intended for the next major release of ETagMW and assumes you are upgrading from the older 1.x usage style to the current API surface.

The biggest changes are:

- middleware configuration is now service-first
- endpoint-level ETag policy is available on .NET 5+
- conditional request handling is part of the runtime behavior
- legacy tag configuration properties are deprecated in favor of delegates

## Summary of breaking changes

### 1. Move configuration out of `UseETag(...)`

The old configuration style used the middleware call itself to pass options:

```csharp
app.UseETag(options =>
{
    options.UseOwnTag = true;
    options.OwnTag = "v1";
});
```

That path is now obsolete.

Use DI registration for configuration and keep the pipeline call parameterless:

```csharp
services.AddETag(options =>
{
    options.ETagFactory = _ => "v1";
});

app.UseETag();
```

What to change:

- replace `app.UseETag(ETagOption)` with `services.AddETag(...)` plus `app.UseETag()`
- replace `app.UseETag(Action<ETagOption>)` with `services.AddETag(...)` plus `app.UseETag()`
- keep `app.UseETag()` in the pipeline, but move configuration to the service container

### 2. Replace `UseOwnTag` and `OwnTag`

`UseOwnTag` and `OwnTag` are obsolete compatibility properties.

Old style:

```csharp
services.AddETag(options =>
{
    options.UseOwnTag = true;
    options.OwnTag = "fixed-tag";
});
```

New style:

```csharp
services.AddETag(options =>
{
    options.ETagFactory = _ => "fixed-tag";
});
```

Why this matters:

- `ETagFactory` supports per-request values
- the middleware normalizes the value into valid entity-tag syntax
- the obsolete properties are the most likely removal candidate in a major release

### 3. Pipeline ordering matters when endpoint metadata is involved

If you use endpoint routing and want per-endpoint ETag policy, `UseRouting()` must run before `UseETag()`.

Use this order:

```csharp
app.UseRouting();
app.UseETag();
app.UseEndpoints(endpoints =>
{
    // map endpoints
});
```

If `UseETag()` runs before routing, endpoint metadata cannot be resolved for the current request.

### 4. Endpoint opt-in and opt-out are now first-class on .NET 5+

The package now supports endpoint metadata helpers for the .NET 5+ targets:

- `WithETag()`
- `WithETag(Action<ETagOption>)`
- `WithoutETag()`
- `[EnableETag]`
- `[DisableETag]`
- `[ETagOptions(typeof(TConfigurator))]`

The `WithETag()` and `WithoutETag()` extensions live in the `Microsoft.AspNetCore.Builder` namespace, so they are available wherever you already have `IEndpointConventionBuilder` in scope. You do not need an extra `using RzR.Web.Middleware.ETag;` to call them.

This enables route-level policy instead of forcing one global behavior for every endpoint.

Example:

```csharp
services.AddETag(options =>
{
    options.ApplyETagByDefault = false;
});

app.MapGet("/products/{id}", (int id) => Results.Text($"product:{id}", "text/plain"))
    .WithETag();

app.MapGet("/health", () => Results.Ok("ok"))
    .WithoutETag();
```

Migration implication:

- if you turn on `ApplyETagByDefault = false`, endpoints without explicit opt-in metadata stop receiving ETags
- `WithETag()` and `WithoutETag()` are available only when your app consumes the .NET 5+ build of the package

### 5. Route-specific option overrides are now isolated per request

Route-level overrides are applied to a cloned `ETagOption` instance. This is the correct behavior, but it matters if you previously assumed route customizations mutated the shared global options.

Example:

```csharp
app.MapPut("/products/{id}", (int id) => Results.Text($"updated:{id}", "text/plain"))
    .WithETag(options =>
    {
        options.ETagFactory = context => $"product:{context.Request.RouteValues["id"]}";
        options.SupportedMethods = new[] { HttpMethods.Put };
    });
```

Migration implication:

- move endpoint-specific behavior into `WithETag(...)` or an attribute-based configurator
- do not rely on route configuration mutating the singleton/global middleware configuration

### 6. Custom MVC and controller overrides now use DI-resolved configurators

For MVC scenarios, the preferred path is `ETagOptionsAttribute(typeof(TConfigurator))` with a configurator implementing `IETagOptionsConfigurator`.

Example:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Attributes;
using RzR.Web.Middleware.ETag.Options;

services.AddETag(options =>
{
    options.ApplyETagByDefault = false;
});
services.AddSingleton<ProductTagConfigurator>();

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    [ETagOptions(typeof(ProductTagConfigurator))]
    public IActionResult Get(int id)
    {
        return Ok(new { id });
    }
}

public sealed class ProductTagConfigurator : IETagOptionsConfigurator
{
    public void Configure(ETagOption options)
    {
        options.ETagFactory = context => $"product:{context.Request.Path}";
    }
}
```

Migration implication:

- register configurator types in DI
- if a configurator is not registered, endpoint execution fails when the middleware tries to resolve it (the configurator is resolved through `GetRequiredService`)

### 7. `IETagOptionsMetadata` alone is not enough in opt-in mode

This is an easy trap when you build custom endpoint metadata.

If `ApplyETagByDefault` is `false`, implementing only `IETagOptionsMetadata` does not opt the route into ETag processing.

You must also provide a policy signal through one of these paths:

- implement `IETagPolicyMetadata` on the same metadata type
- add `WithETag()` to the endpoint
- add `[EnableETag]`
- use `[ETagOptions(typeof(TConfigurator))]`

Without that policy signal, the middleware skips the endpoint.

### 8. Conditional request handling changes observed behavior

The middleware now does more than stamp an `ETag` header. It evaluates conditional headers against the current validator state.

Current behavior:

- matching `If-None-Match` on `GET` or `HEAD` returns `304 Not Modified`
- matching `If-None-Match` on unsafe methods returns `412 Precondition Failed`
- non-matching `If-Match` returns `412 Precondition Failed`
- `LastModifiedFactory` enables `Last-Modified` output and `If-Modified-Since` handling
- `If-Modified-Since` is evaluated only when the request does not also send `If-None-Match`; `If-None-Match` always wins
- `HEAD` is honored for `304` only when it is included in `SupportedMethods`; the default set contains `GET` only

Migration implication:

- update integration tests that previously asserted only `200 OK`
- audit clients that send conditional headers, because they may now receive `304` or `412`

### 9. Eligibility rules are stricter and more explicit

An ETag is generated only when the request and response are eligible.

By default:

- only `GET` is included in `SupportedMethods`
- only `200 OK` responses are tagged
- responses that already carry an `ETag` header are left untouched
- range requests are skipped
- WebSocket requests are skipped
- `text/event-stream` responses are skipped
- responses larger than `MaxBodySize` are skipped

Migration implication:

- if you expect validators on `PUT`, `HEAD`, or other methods, explicitly extend `SupportedMethods`
- if your payloads are large, set `MaxBodySize` high enough or expect ETag generation to be bypassed

## Recommended migration path

### Step 1. Move all middleware configuration to service registration

```csharp
services.AddETag(options =>
{
    options.MaxBodySize = 256 * 1024;
    options.ETagFactory = context => $"resource:{context.Request.Path}";
});
```

### Step 2. Keep the middleware call simple

```csharp
app.UseETag();
```

### Step 3. Decide whether you want global or opt-in behavior

Use the default global mode:

```csharp
services.AddETag();
```

Use opt-in mode:

```csharp
services.AddETag(options =>
{
    options.ApplyETagByDefault = false;
});
```

Then add per-endpoint metadata where needed.

### Step 4. Replace obsolete static tag settings

```csharp
options.ETagFactory = _ => "fixed-tag";
```

### Step 5. Add `LastModifiedFactory` if your resources have a stable timestamp

```csharp
options.LastModifiedFactory = _ => DateTimeOffset.UtcNow;
```

If you do this, update tests to cover `If-Modified-Since` behavior.

## Before and after example

### Before

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseRouting();

    app.UseETag(options =>
    {
        options.UseOwnTag = true;
        options.OwnTag = "v1";
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGet("/etag", async context =>
        {
            await context.Response.WriteAsync("payload");
        });
    });
}
```

### After

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRouting();
    services.AddETag(options =>
    {
        options.ETagFactory = _ => "v1";
    });
}

public void Configure(IApplicationBuilder app)
{
    app.UseRouting();
    app.UseETag();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGet("/etag", async context =>
        {
            await context.Response.WriteAsync("payload");
        });
    });
}
```
