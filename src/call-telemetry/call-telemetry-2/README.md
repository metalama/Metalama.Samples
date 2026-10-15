---
uid: sample-call-telemetry-2
level: 300
summary: "This example creates an ActivitySource span around each call to a third-party SDK with a T# template interceptor, handles async methods, and passes the calling operation as an added parameter so that the call sites share the generated methods."
keywords: "interceptors, tracing, OpenTelemetry, ActivitySource, Activity, span, async, T# template, RedirectToSynthesizedMethod, PlaceInType, Configure, AddParameter, SetArgument, project fabric"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Call telemetry example, step 2: Producing trace spans for SDK calls

[!metalama-project-buttons .]

Distributed tracing shows how much time an application spends in each call to an external service. The .NET API for
tracing is `System.Diagnostics.ActivitySource`. Each `Activity` that it starts is a span, which OpenTelemetry exports
to a tracing back-end.

In this example, the application calls `PaymentClient`, a class of a separate class library that represents a
third-party SDK. The SDK does not produce spans. A project fabric creates a span around each call to the methods of
`PaymentClient`. The calling code does not change in the source code:

[!metalama-compare CheckoutService.cs]

The program writes each span to the console when the span ends. The durations change from one run to another.

```
[span] PaymentClient.GetFee, caller: PriceList.GetPriceWithFee, status: Unset, duration: 1.4 ms
Price with fee: 103.20
[span] PaymentClient.GetFee, caller: CheckoutService.CheckoutAsync, status: Unset, duration: 0.0 ms
[span] PaymentClient.AuthorizeAsync, caller: CheckoutService.CheckoutAsync, status: Unset, duration: 23.4 ms
[span] PaymentClient.CaptureAsync, caller: CheckoutService.CheckoutAsync, status: Unset, duration: 10.5 ms
Authorization: AUTH-1111-103.20
[span] PaymentClient.GetFee, caller: CheckoutService.CheckoutAsync, status: Unset, duration: 0.0 ms
[span] PaymentClient.AuthorizeAsync, caller: CheckoutService.CheckoutAsync, status: Error, duration: 21.1 ms
Checkout failed: The card was declined.
```

A real application does not use an `ActivityListener`. Instead, it configures OpenTelemetry to export the spans of the
`Metalama.Samples.CallTelemetry.Payments` source, for instance with `AddSource( "Metalama.Samples.CallTelemetry.Payments" )`.

## Implementation

The `PaymentsTelemetry` class holds the `ActivitySource`:

[!metalama-file PaymentsTelemetry.cs]

The following project fabric registers the interceptors:

[!metalama-file TelemetryFabric.cs]

The `AmendProject` method registers the interceptors with the `InterceptMethods` method:

* `Type( typeof(PaymentClient) ).AllMethods()` selects all the methods that `PaymentClient` declares.
* `RedirectToSynthesizedMethod( nameof(this.Trace) )` generates the interceptor methods from the `Trace` template.
* `PlaceInType( typeof(PaymentsTelemetry) )` generates the interceptor methods in the `PaymentsTelemetry` class,
  instead of the default `MetalamaInterceptors` class. The template can then access the `ActivitySource` field
  through `meta.ThisType`.
* `Configure` adds the `operation` parameter to each generated method, and gives its value for each call site: the
  name of the calling type and member, read from `method.Context.Caller`.

### Sharing the interceptors between call sites

The template needs the name of the calling operation. It could read this name at compile time with
`meta.Interception.CallingMember`, after `WithGranularity( InterceptorGranularity.Member )`. Metalama would then
generate one interceptor for each calling member, because the code of each interceptor would contain a different
name.

Instead, the name is a run-time parameter of the template, `operation`, and each call site passes its value. The code
of the interceptor is then the same for all the call sites of a method, and Metalama generates a single interceptor
for each method of `PaymentClient`. For instance, `PriceList.GetPriceWithFee` and `CheckoutService.CheckoutAsync`
call the same `PaymentClient_GetFee_Interceptor` method.

### Async methods

`AuthorizeAsync` and `CaptureAsync` return a `Task`. For these methods, Metalama generates an `async` interceptor, and
`meta.Proceed()` awaits the call. The span therefore ends when the asynchronous operation completes, not when the
method returns the task. The template does not need specific code for async methods.

When the call throws an exception, the template sets the status of the span to `Error`, then rethrows the exception.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
> <xref:interceptor-deduplication-rules>
