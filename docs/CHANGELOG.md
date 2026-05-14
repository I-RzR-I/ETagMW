### **v2.0.0.8350** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 14-05-2026
* -> [DEV] - `services.AddETag(...)` is now the supported registration path. `app.UseETag(ETagOption)` and `app.UseETag(Action<ETagOption>)` are marked `[Obsolete]`.
* -> [DEV] - `ETagOption.UseOwnTag` / `OwnTag` deprecated — use `ETagFactory` instead.
* -> [DEV] - Per-endpoint policy is no longer inferred. Under `ApplyETagByDefault = false`, routes must opt in via `EnableETagAttribute`, `[ETagOptions(typeof(...))]`, or `WithETag()` (otherwise silently skipped).
* -> [DEV] - `EndpointConventionBuilderExtensions` moved to the `Microsoft.AspNetCore.Builder` namespace (test contract enforces this).
* -> [DEV] - `CanProcessEtag` no longer inspects endpoint metadata — endpoint resolution happens once via `HttpRequestExtensions.ResolveETagResolution`.
* -> [DEV] - **DI-first registration:** `AddETag()`, `AddETag(Action<ETagOption>)`, `AddETag(IConfiguration)` with binding for `ApplyETagByDefault`, `MaxBodySize`, `SupportedMethods`.
* -> [DEV] - **Endpoint metadata model:** `IETagPolicyMetadata`, `IETagOptionsMetadata`, `ETagOptionsMetadata`, plus `EnableETagAttribute` / `DisableETagAttribute` / `ETagOptionsAttribute`.
* -> [DEV] - **Minimal API helpers** (`NET5_0_OR_GREATER`): `WithETag()`, `WithETag(Action<ETagOption>)`, `WithoutETag()` — clones the global option per route to avoid singleton mutation.
* -> [DEV] - **MVC parity:** `IETagOptionsConfigurator` + `IETagOptionsConfiguratorMetadata` resolved from `HttpContext.RequestServices` (configurator types must be DI-registered).
* -> [DEV] - **Pluggable generators:** `IETagGenerator` with `Sha256ETagGenerator` (default) and `XxHashETagGenerator` (net6+ via `System.IO.Hashing`).
* -> [DEV] - **Request hooks:** `ETagFactory` and `LastModifiedFactory` for explicit ETag / `Last-Modified` resolution; `If-Modified-Since` honored → `304`.
* -> [DEV] - **Conditional request semantics:** `If-Match` / `If-None-Match` on writes return `412 Precondition Failed`; `304` clears body/content headers.
* -> [DEV] - **Buffering:** `ResponseBufferingStream` + `BufferingResponseFeature` + `BufferingStreamManager` backed by `Microsoft.IO.RecyclableMemoryStream`; respects `MaxBodySize`, bypasses on `Range` / WebSocket / large payloads. `BodyWriter` writes are captured.
* -> [DEV] - **Observability:** `ETagMiddlewareMetrics` recorder registered as singleton.

### **v1.0.2.1758** <br/>
-> Fix wrong modification.

### **v1.0.1.1758** <br/>
-> Update lib version.
