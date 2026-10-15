---
uid: sample-universal-interceptor-2
level: 300
summary: "This example extends the universal interceptor to methods that return a Task, a Task<T>, a ValueTask or a ValueTask<T>, with InvokeAsync overloads and async templates."
keywords: "interceptors, universal interceptor, async, Task, ValueTask, InvokeAsync, async template, value tuple"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Universal interceptor example, step 2: Handling async methods

[!metalama-project-buttons weather-app]

The `Invoke` method of <xref:sample-universal-interceptor-1> returns the result of the call. For an async method, this
result is a task, so `Invoke` would return before the operation completes, and it could not observe the result or the
exception of the operation. This article explains how the async methods of `WeatherClient` reach the interceptor.

The `DescribeForecastAsync` method of `WeatherReport` calls methods that return a `Task`, a `ValueTask`, a `Task<T>` and
a `ValueTask<T>`:

[!metalama-file weather-app/WeatherReport.cs marker="AsyncCalls"]

When the program calls `DescribeForecastAsync`, `CallLog` writes the following lines:

```
[call] WeatherClient.RefreshAsync(CancellationToken) succeeded asynchronously.
[call] WeatherClient.ClearCacheAsync(CancellationToken) succeeded asynchronously.
[call] WeatherClient.GetForecastAsync(Prague, 3, CancellationToken) returned Forecast { City = Prague, Days = 3, AverageHigh = 13.5 } asynchronously.
[call] WeatherClient.GetTemperatureAsync(Prague, CancellationToken) returned 12.5 asynchronously.
Prague: 12.5 now, 13.5 on average in the next 3 days
```

The `cancellationToken` parameters are optional, and the call sites do not pass them. The generated interceptor has
the same parameters as the intercepted method, so the call site passes `default(CancellationToken)`, and the tuple
contains it.

## The run-time interface

`IUniversalInterceptor` has two `InvokeAsync` overloads, one for `Task<TResult>` and one for `ValueTask<TResult>`:

[!metalama-file weather-app/IUniversalInterceptor.cs marker="InvokeAsync"]

A method that returns a `Task` or a `ValueTask` has no result, so the generated code uses `ValueTuple` as `TResult`, as
for a void method. Two overloads thus handle the four return types.

The `proceed` delegate is a `Func<TArgs, Task<TResult>>` or a `Func<TArgs, ValueTask<TResult>>`, and it receives the tuple
by value. An async method cannot have `ref` parameters, so the delegate cannot write the arguments back to the tuple.
For this reason, the methods that return an awaitable type and have `ref` or `out` parameters are not intercepted. See
<xref:sample-universal-interceptor-3>.

## The logger

`CallLog` implements `InvokeAsync` with `async` methods. It awaits `proceed`, so it writes the result or the exception
when the operation completes:

[!metalama-file weather-app/CallLog.cs member="CallLog.InvokeAsync"]

## Choosing the template

For a method that returns an awaitable type, `RedirectToUniversalInterceptor` chooses one of four templates from the
return type. The result type is the type of the awaited value, or `null` for a `Task` or a `ValueTask`. Other awaitable
types are not supported, and the call is not intercepted:

[!metalama-file weather-app/UniversalInterceptorExtensions.cs member="UniversalInterceptorExtensions.TryChooseAsyncTemplate"]

## The templates

The template for `Task<T>` awaits `InvokeAsync`. Its local function returns the task of the intercepted method without
awaiting it:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.InvokeTaskOfTAsync"]

The template for `Task` awaits `InvokeAsync` too. Its local function must return a `Task<ValueTuple>`, so it awaits the
task of the intercepted method and returns `default( ValueTuple )`:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.InvokeTaskAsync"]

The templates for `ValueTask<T>` and `ValueTask` are identical, with `ValueTask` instead of `Task`.

The templates must be `async` methods. Metalama generates an `async` interceptor for any method that returns an
awaitable type, so a template that returns the task directly produces code that does not compile.

The templates generate the following interceptors:

```csharp
internal static async ValueTask WeatherClient_ClearCacheAsync_Interceptor(WeatherClient receiver, CancellationToken cancellationToken)
{
    var args = (receiver, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "ClearCacheAsync", "WeatherClient.ClearCacheAsync(CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static async ValueTask<ValueTuple> ProceedAsync((WeatherClient receiver, CancellationToken cancellationToken) receivedArgs)
    {
        await receivedArgs.receiver.ClearCacheAsync(receivedArgs.cancellationToken);
        return default;
    }
}
internal static async Task<Forecast> WeatherClient_GetForecastAsync_Interceptor(WeatherClient receiver, string city, int days, CancellationToken cancellationToken)
{
    var args = (receiver, city, days, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetForecastAsync", "WeatherClient.GetForecastAsync(string, int, CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    return await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static Task<Forecast> ProceedAsync((WeatherClient receiver, string city, int days, CancellationToken cancellationToken) receivedArgs)
    {
        return receivedArgs.receiver.GetForecastAsync(receivedArgs.city, receivedArgs.days, receivedArgs.cancellationToken);
    }
}
internal static async ValueTask<double> WeatherClient_GetTemperatureAsync_Interceptor(WeatherClient receiver, string city, CancellationToken cancellationToken)
{
    var args = (receiver, city, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetTemperatureAsync", "WeatherClient.GetTemperatureAsync(string, CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    return (double)await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static ValueTask<double> ProceedAsync((WeatherClient receiver, string city, CancellationToken cancellationToken) receivedArgs)
    {
        return receivedArgs.receiver.GetTemperatureAsync(receivedArgs.city, receivedArgs.cancellationToken);
    }
}
internal static async Task WeatherClient_RefreshAsync_Interceptor(WeatherClient receiver, CancellationToken cancellationToken)
{
    var args = (receiver, cancellationToken);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "RefreshAsync", "WeatherClient.RefreshAsync(CancellationToken)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    await interceptor.InvokeAsync(args, metadata, ProceedAsync);
    static async Task<ValueTuple> ProceedAsync((WeatherClient receiver, CancellationToken cancellationToken) receivedArgs)
    {
        await receivedArgs.receiver.RefreshAsync(receivedArgs.cancellationToken);
        return default;
    }
}
```

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
> <xref:interceptors-selecting-calls>
