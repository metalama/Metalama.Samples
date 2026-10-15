---
uid: sample-call-telemetry
summary: "How to audit the calls to sensitive APIs and produce telemetry for the calls to a third-party SDK, including the calls added later, without editing the call sites, using Metalama interceptors."
keywords: "interceptors, audit, telemetry, OpenTelemetry, ActivitySource, System.Diagnostics.Metrics, third-party SDK, project fabric, Metalama.Extensions.Interceptors"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Example: auditing and monitoring calls to external APIs

A team must often audit every call to a sensitive API, such as `File.Delete` or `Process.Start`, or produce telemetry
for every call to a third-party SDK. The rule applies to the calls that exist today and to the calls that developers
add later. Adding audit or telemetry code at each call site is repetitive, and a developer who adds a call can forget
it.

With interceptors, the rule is written once, in a project fabric. Metalama rewrites every matching call site of the
project at build time, including the call sites added later. The calling code does not change in the source code, and
the intercepted methods can be in another assembly, such as the .NET libraries or a NuGet package.

This series uses `PaymentClient`, a class of a separate class library, to represent the third-party SDK. Each call
to the SDK is rewritten to call a generated interceptor, which creates a trace span and records metrics:

[!metalama-compare call-telemetry-3/CheckoutService.cs]

## In this series

| Article | Description |
|---------|-------------|
| <xref:sample-call-telemetry-1> | Static methods that you write audit the calls to `File.Delete` and `Process.Start` in the whole project. |
| <xref:sample-call-telemetry-2> | A T# template creates an `ActivitySource` span around each call to the SDK. |
| <xref:sample-call-telemetry-3> | The template also records a counter and a duration histogram with `System.Diagnostics.Metrics`. |

## When to use a hook of the library instead

Some libraries provide a hook that sees every call, including the calls made by the library itself and by the
framework. When such a hook exists, prefer it to interceptors:

* For `HttpClient`, add a `DelegatingHandler` to the handler pipeline, or use the telemetry that `HttpClient` already
  emits.
* For Entity Framework Core, register an EF Core interceptor, such as a `DbCommandInterceptor`.
* For libraries that publish events with `DiagnosticSource`, `ActivitySource` or `Meter`, subscribe to these events,
  for instance with the OpenTelemetry instrumentation packages.

Interceptors only rewrite the calls that are in the source code of the current project. They do not see the calls
made inside the library, by the framework, or through reflection. Use interceptors when the library has no such hook,
when the API is static, such as `File.Delete`, or when the telemetry must know the calling code.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:interceptors-use-cases>
> <xref:fabrics>
