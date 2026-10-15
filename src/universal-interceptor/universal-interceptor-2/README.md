---
uid: sample-universal-interceptor-2
level: 300
summary: "This example extends the universal interceptor to methods with ref and out parameters and to async methods, and skips the calls on struct receivers with ForEachSite."
keywords: "interceptors, universal interceptor, ref parameter, out parameter, async, Task, ValueTask, struct receiver, ForEachSite, Skip, value tuple"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Universal interceptor example, step 2: Supporting ref, out and async methods

[!metalama-project-buttons .]

In this step, the application also calls methods of `WeatherClient` that have `ref` or `out` parameters, and methods
that return a `Task`, a `Task<T>`, a `ValueTask` or a `ValueTask<T>`. `CallLog` now logs the asynchronous calls, and
the arguments that a call changes:

[!metalama-compare WeatherReport.cs]

When the program runs, `CallLog` writes the following lines:

```
[call] WeatherClient.TryGetHumidity(Prague, 0) returned True, and changed the arguments to (Prague, 71).
Prague: humidity 71 %
[call] WeatherClient.TryGetHumidity(Lisbon, 0) returned False.
Lisbon: humidity unknown
[call] WeatherClient.GetAlerts(0, 2) returned [Lisbon: strong wind on Monday., Prague: frost on Tuesday.], and changed the arguments to (2, 2).
Alerts: Lisbon: strong wind on Monday. Prague: frost on Tuesday.
[call] WeatherClient.GetAlerts(2, 2) returned [Prague: heavy rain on Thursday.], and changed the arguments to (3, 2).
Alerts: Prague: heavy rain on Thursday.
[call] WeatherClient.GetAlerts(3, 2) returned [].
[call] WeatherClient.RefreshAsync(CancellationToken) succeeded asynchronously.
[call] WeatherClient.ClearCacheAsync(CancellationToken) succeeded asynchronously.
[call] WeatherClient.GetForecastAsync(Prague, 3, CancellationToken) returned Forecast { City = Prague, Days = 3, AverageHigh = 13.5 } asynchronously.
[call] WeatherClient.GetTemperatureAsync(Prague, CancellationToken) returned 12.5 asynchronously.
Prague: 12.5 now, 13.5 on average in the next 3 days
[call] WeatherClient.GetQuota() returned 2 requests remaining.
Request sent, 1 requests remaining.
Request sent, 0 requests remaining.
```

The calls to `RequestQuota.TryConsume` are not in the log, because they are not intercepted. See
[Calls on a struct receiver](#calls-on-a-struct-receiver) below.

## Writing back ref and out arguments

The generated interceptor passes the tuple to `Invoke` by reference. The `Proceed` delegate passes the elements of the
tuple to the `ref` and `out` parameters of the intercepted method, so the method writes its results to the tuple.
After the call, the interceptor assigns the elements of the tuple to its own `ref` and `out` parameters, which are the
variables of the call site. For instance, the interceptor of `TryGetHumidity` is:

```csharp
internal static bool WeatherClient_TryGetHumidity_Interceptor(WeatherClient receiver, string city, out int humidity)
{
    var args = (receiver, city, humidity: default(int));
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "TryGetHumidity", "WeatherClient.TryGetHumidity(string, out int)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    try
    {
        return interceptor.Invoke(ref args, metadata, Proceed);
    }
    finally
    {
        humidity = args.humidity;
    }

    static bool Proceed(ref (WeatherClient receiver, string city, int humidity) receivedArgs)
    {
        return receivedArgs.receiver.TryGetHumidity(receivedArgs.city, out receivedArgs.humidity);
    }
}
```

An `out` parameter has no value before the call, so its element in the tuple is initialized with the default value.
The `Invoke` and `InvokeVoid` templates generate the `try` and `finally` blocks only when the method has `ref` or `out`
parameters.

The run-time object can also read and change the arguments, because it receives the tuple by reference. `CallLog`
formats the arguments before and after the call, and writes them again when they are different.

## Async methods

When the intercepted method returns an awaitable type, Metalama generates an `async` interceptor method. The templates
`InvokeTaskOfTAsync`, `InvokeTaskAsync`, `InvokeValueTaskOfTAsync` and `InvokeValueTaskAsync` await the `InvokeAsync`
method of the interceptor, which has two overloads: one for `Task<TResult>` and one for `ValueTask<TResult>`. A method
that returns a `Task` or a `ValueTask` without a result uses `ValueTuple` as `TResult`.

`InvokeAsync` receives the tuple by value, because an async method cannot have `ref` parameters. For the same reason,
methods that return an awaitable type and have `ref` or `out` parameters are not intercepted.

The `CallLog.InvokeAsync` methods are `async` methods, so they write the result after the asynchronous operation
completes.

[!metalama-file CallLog.cs]

## Calls on a struct receiver

`WeatherClient.GetQuota` returns a `RequestQuota`, a mutable struct of the weather library. Its `TryConsume` method
changes the instance on which it is called. If the call were intercepted, the tuple would contain a copy of the
`quota` variable, `TryConsume` would change the copy, and the loop of `PrintRemainingRequests` would never end.

Therefore, `RedirectToUniversalInterceptor` does not intercept a call on a struct receiver. It reports the warning
`UNIV05` and leaves the call unchanged. The test `StructReceiverWarning` of the
`Metalama.Samples.UniversalInterceptor2.Tests` project shows this warning.

The fabric of this step intercepts the calls to all the types of the weather library, including `RequestQuota`. To
remove the warning, it decides for each call site with `ForEachSite`, and returns `site.Skip()` for the calls on a
struct receiver:

[!metalama-file InterceptionFabric.cs]

Calls to static methods of a struct are not concerned, because they have no receiver.

> [!div class="see-also"]
> <xref:interceptors-selecting-calls>
> <xref:generating-interceptors-with-templates>
> <xref:interceptor-binding-rules>
