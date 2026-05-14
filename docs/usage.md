# Usage

ETagMW is configured in two steps:

1. Register services with `AddETag(...)`.
2. Add the middleware with `UseETag()`.

Prefer this DI-based setup. The older `UseETag(ETagOption)` and `UseETag(Action<ETagOption>)` overloads still exist for backward compatibility, but they are obsolete.

## Basic setup

### Startup pattern

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RzR.Web.Middleware.ETag;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        services.AddETag();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseETag();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/etag", async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("payload");
            });
        });
    }
}
```

When you use explicit endpoint routing, call `UseRouting()` before `UseETag()`. That order is required if you want per-endpoint ETag metadata to be honored.

### Minimal hosting

```csharp
using RzR.Web.Middleware.ETag;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddETag();

var app = builder.Build();

app.UseETag();

app.MapGet("/etag", () => Results.Text("payload", "text/plain"));

app.Run();
```

## Available options

`ETagOption` exposes the following configuration points:

| Option | Description |
| --- | --- |
| `ETagFactory` | Returns a request-specific entity tag. The middleware normalizes the value into valid ETag syntax. |
| `LastModifiedFactory` | Returns a request-specific `Last-Modified` value. Matching `If-Modified-Since` requests can then return `304 Not Modified`. |
| `SupportedMethods` | HTTP methods eligible for processing. The default is `GET` only. |
| `MaxBodySize` | Maximum buffered response size, in bytes, used for ETag generation. The default is `1048576` (1 MB). |
| `ApplyETagByDefault` | Enables or disables ETag processing for eligible endpoints by default. The default is `true`. |
| `UseOwnTag` / `OwnTag` | Obsolete compatibility settings. Use `ETagFactory` instead. |

### Configure in code

```csharp
services.AddETag(options =>
{
    options.MaxBodySize = 256 * 1024;
    options.ETagFactory = context => $"resource:{context.Request.Path}";
    options.LastModifiedFactory = _ => DateTimeOffset.UtcNow;
    options.SupportedMethods = new[] { HttpMethods.Get, HttpMethods.Head };
});
```

### Bind from configuration

```json
{
  "ETag": {
    "ApplyETagByDefault": false,
    "MaxBodySize": 262144,
    "SupportedMethods": ["GET", "HEAD"]
  }
}
```

```csharp
services.AddETag(Configuration.GetSection("ETag"));
```

Configuration binding covers `ApplyETagByDefault`, `MaxBodySize`, and `SupportedMethods`. Delegate-based options such as `ETagFactory` and `LastModifiedFactory` must be configured in code.

## Custom ETag generation

The default generator is `Sha256ETagGenerator`. You can replace it by registering your own `IETagGenerator` implementation.

```csharp
using System.IO;
using Microsoft.AspNetCore.Http;
using RzR.Web.Middleware.ETag.Abstractions;

public sealed class CustomETagGenerator : IETagGenerator
{
    public string Generate(HttpContext context, Stream responseBody)
    {
        return $"custom:{context.Request.Path}";
    }
}

services.AddETag();
services.AddSingleton<IETagGenerator, CustomETagGenerator>();
```

## Per-endpoint policy on .NET 5+

When your application consumes the .NET 5 or later build of the package, you can control ETag behavior per endpoint.

### Minimal API route helpers

`WithETag()` explicitly enables ETag processing for a route.

`WithETag(Action<ETagOption>)` enables processing and applies route-specific option overrides.

`WithoutETag()` explicitly disables processing for a route.

```csharp
using Microsoft.AspNetCore.Http;
using RzR.Web.Middleware.ETag;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddETag(options =>
{
    options.ApplyETagByDefault = false;
});

var app = builder.Build();

app.UseETag();

app.MapGet("/products/{id}", (int id) => Results.Text($"product:{id}", "text/plain"))
    .WithETag();

app.MapPut("/products/{id}", (int id) => Results.Text($"updated:{id}", "text/plain"))
    .WithETag(options =>
    {
        options.ETagFactory = context => $"product:{context.Request.RouteValues["id"]}";
        options.SupportedMethods = new[] { HttpMethods.Put };
    });

app.MapGet("/health", () => Results.Ok("ok"))
    .WithoutETag();

app.Run();
```

### MVC and controller attributes

For MVC actions or controllers, you can use metadata attributes instead of route builder helpers.

- `EnableETagAttribute` enables ETag processing.
- `DisableETagAttribute` disables ETag processing.
- `ETagOptionsAttribute(typeof(TConfigurator))` enables ETag processing and applies per-request overrides through a DI-resolved configurator.

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

    [HttpGet("health")]
    [DisableETag]
    public IActionResult Health()
    {
        return Ok("ok");
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

If `ApplyETagByDefault` is `false`, attaching only custom `IETagOptionsMetadata` is not enough. You still need an explicit policy signal such as `WithETag()`, `EnableETagAttribute`, or `ETagOptionsAttribute`.

## Conditional request behavior

The middleware evaluates validators after the response body has been produced and buffered.

- Eligible requests receive an `ETag` header only when the final response status is `200 OK`.
- If `LastModifiedFactory` returns a value, the middleware also writes `Last-Modified`.
- A matching `If-None-Match` on `GET` or `HEAD` returns `304 Not Modified` with body-specific headers removed.
- A matching `If-None-Match` on an unsafe method returns `412 Precondition Failed`.
- A non-matching `If-Match` returns `412 Precondition Failed`.
- A matching `If-Modified-Since` returns `304 Not Modified` for safe methods when `Last-Modified` is configured.

## When ETag generation is skipped

ETag generation is skipped when any of the following is true:

- the request method is not in `SupportedMethods`
- the request contains a `Range` header
- the request is a WebSocket request
- the response status is not `200 OK`
- the response already contains an `ETag` header
- the buffered response body exceeds `MaxBodySize`
- the response content type is `text/event-stream`

## Notes

- Route-level overrides are applied to a cloned per-request option instance, so route customization does not mutate the shared global middleware configuration.
- If you use explicit routing middleware, endpoint metadata requires `UseRouting()` before `UseETag()`.
- Prefer `services.AddETag(...)` plus `app.UseETag()` over the obsolete `UseETag(...)` configuration overloads.
