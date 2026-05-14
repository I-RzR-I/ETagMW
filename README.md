> This package targets .NET Standard 2.0+ and .NET 5.0 or later.

[![NuGet Version](https://img.shields.io/nuget/v/RzR.Web.Middleware.ETag.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Web.Middleware.ETag/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/RzR.Web.Middleware.ETag.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Web.Middleware.ETag)

<details>

  <summary>Old version</summary>
  
[![NuGet Version](https://img.shields.io/nuget/v/ETagMW.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ETagMW/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/ETagMW.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/ETagMW)

</details>

`RzR.Web.Middleware.ETag` is ASP.NET Core middleware that adds HTTP validators to eligible responses and evaluates conditional request headers against the current response state.

It supports:
- automatic ETag generation from the response body
- request-specific `ETag` and `Last-Modified` values through delegates
- conditional request handling for `If-Match`, `If-None-Match`, and `If-Modified-Since`
- DI-based replacement of the ETag generator through `IETagGenerator`
- endpoint-level opt-in, opt-out, and route-specific overrides on .NET 5+

## Supported frameworks

Endpoint metadata features such as `WithETag()`, `WithoutETag()`, and `ETagOptionsAttribute` are available when your application consumes the .NET 5+ build of the package.

## Installation

```powershell
Install-Package RzR.Web.Middleware.ETag
```

```bash
dotnet add package RzR.Web.Middleware.ETag
```

## Quick start

Prefer configuring the middleware in DI with `AddETag(...)`, then add `UseETag()` to the pipeline.

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

When you use explicit endpoint routing, place `UseETag()` after `UseRouting()` and before `UseEndpoints(...)` so endpoint metadata can be resolved correctly.

## Common configuration

```csharp
services.AddETag(options =>
{
	options.MaxBodySize = 1024 * 1024;
	options.ETagFactory = context => $"resource:{context.Request.Path}";
	options.LastModifiedFactory = _ => DateTimeOffset.UtcNow;
});
```

Default behavior:

- only `GET` requests are processed unless you extend `SupportedMethods`
- only `200 OK` responses receive an `ETag`
- range requests, WebSocket requests, server-sent events, and oversized buffered bodies are skipped
- a matching `If-None-Match` on `GET` or `HEAD` produces `304 Not Modified`
- a failing `If-Match`, or a matching `If-None-Match` on an unsafe method, produces `412 Precondition Failed`

## Documentation

1. [USING](docs/usage.md)
2. [CHANGELOG](docs/CHANGELOG.md)
3. [MIGRATION](docs/migartion.md)
