---
uid: sample-call-telemetry-3
browse-source: .
level: 300
summary: "This example records a counter and a duration histogram of System.Diagnostics.Metrics for each call to a third-party SDK, tagged with the method and the calling type, with a T# template interceptor."
keywords: "interceptors, metrics, OpenTelemetry, System.Diagnostics.Metrics, Meter, Counter, Histogram, TagList, T# template, Configure, AddParameter, project fabric"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Call telemetry example, step 3: Recording metrics

[!metalama-project-buttons .]

Spans describe individual calls. To monitor the number of calls and their duration over time, an application also
records metrics. The .NET API for metrics is `System.Diagnostics.Metrics`. A `Meter` creates instruments, such as a
`Counter<T>` and a `Histogram<T>`, and OpenTelemetry exports their measurements to a monitoring back-end.

In this step, the interceptors of the previous step also record two metrics for each call to `PaymentClient`:

* `payments.client.calls`, a counter of the calls.
* `payments.client.duration`, a histogram of the duration of the calls, in milliseconds.

Both metrics are tagged with the called method and the calling type. The calling code does not change in the source
code:

[!metalama-compare CheckoutService.cs]

The program writes each measurement to the console. The durations change from one run to another.

```
[metric] payments.client.calls 1 {method=PaymentClient.GetFee, caller.type=PriceList}
[metric] payments.client.duration 8.7 ms {method=PaymentClient.GetFee, caller.type=PriceList}
Price with fee: 103.20
[metric] payments.client.calls 1 {method=PaymentClient.GetFee, caller.type=CheckoutService}
[metric] payments.client.duration 0 ms {method=PaymentClient.GetFee, caller.type=CheckoutService}
[metric] payments.client.calls 1 {method=PaymentClient.AuthorizeAsync, caller.type=CheckoutService}
[metric] payments.client.duration 50 ms {method=PaymentClient.AuthorizeAsync, caller.type=CheckoutService}
[metric] payments.client.calls 1 {method=PaymentClient.CaptureAsync, caller.type=CheckoutService}
[metric] payments.client.duration 9.8 ms {method=PaymentClient.CaptureAsync, caller.type=CheckoutService}
Authorization: AUTH-1111-103.20
[metric] payments.client.calls 1 {method=PaymentClient.GetFee, caller.type=CheckoutService}
[metric] payments.client.duration 0 ms {method=PaymentClient.GetFee, caller.type=CheckoutService}
[metric] payments.client.calls 1 {method=PaymentClient.AuthorizeAsync, caller.type=CheckoutService}
[metric] payments.client.duration 21 ms {method=PaymentClient.AuthorizeAsync, caller.type=CheckoutService}
Checkout failed: The card was declined.
```

A real application does not use a `MeterListener`. Instead, it configures OpenTelemetry to export the metrics of the
`Metalama.Samples.CallTelemetry.Payments` meter, for instance with `AddMeter( "Metalama.Samples.CallTelemetry.Payments" )`.

## Implementation

The `PaymentsTelemetry` class now also holds the `Meter` and its instruments:

[!metalama-file PaymentsTelemetry.cs]

The fabric is the same as in the previous step, with two changes:

[!metalama-file TelemetryFabric.cs]

* `Configure` adds a second parameter, `callingType`, whose value is the name of the calling type.
* The `Monitor` template measures the duration of the call with `Stopwatch.GetTimestamp`. It records the metrics in a
  `finally` block, so that the failed calls are also counted. For async methods, `meta.Proceed()` awaits the call, so
  the duration covers the whole asynchronous operation.

The name of the called method is known at compile time, so the template writes it as a constant. The name of the
calling type is a run-time parameter, so the call sites of a method still share the same interceptor.

The metrics are tagged with the calling type, and the span is tagged with the calling operation. The monitoring
back-end creates a separate time series for each distinct combination of tag values. A tag with fewer distinct
values, such as the calling type, keeps the number of time series small.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
> <xref:sample-call-telemetry-2>
