---
uid: sample-universal-interceptor-3
level: 300
summary: "This example extends the universal interceptor to methods with ref and out parameters, whose arguments are passed in the value tuple by reference and written back to the call site after the call."
keywords: "interceptors, universal interceptor, ref parameter, out parameter, value tuple, write back, T# template"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Universal interceptor example, step 3: Handling ref and out parameters

[!metalama-project-buttons weather-app]

The generated interceptor copies the arguments into a tuple, and the intercepted method receives the elements of this
tuple. When a parameter is `ref` or `out`, the intercepted method writes to the element of the tuple, not to the
variable of the caller. This article explains how these values are written back to the call site.

The `WeatherReport` class calls a method with an `out` parameter, `TryGetHumidity`, and a method with a `ref`
parameter, `GetAlerts`, which moves a cursor after the returned alerts:

[!metalama-file weather-app/WeatherReport.cs marker="RefOutCalls"]

When the program calls these methods, `CallLog` writes the arguments before and after each call:

```
[call] WeatherClient.TryGetHumidity(Prague, 0) returned True, and changed the arguments to (Prague, 71).
Prague: humidity 71 %
[call] WeatherClient.TryGetHumidity(Lisbon, 0) returned False.
Lisbon: humidity unknown
[call] WeatherClient.GetAlerts(0, 2) returned [Lisbon: strong wind on Monday., Prague: frost on Tuesday.], and changed the arguments to (2, 2).
[call] WeatherClient.GetAlerts(2, 2) returned [Prague: heavy rain on Thursday.], and changed the arguments to (3, 2).
[call] WeatherClient.GetAlerts(3, 2) returned [].
Alerts: Lisbon: strong wind on Monday. Prague: frost on Tuesday. Prague: heavy rain on Thursday.
```

## Passing the tuple by reference

The `Invoke` method of `IUniversalInterceptor` receives the tuple by reference, and passes it by reference to the
delegate:

[!metalama-file weather-app/IUniversalInterceptor.cs marker="Invoke"]

[!metalama-file weather-app/InterceptorDelegate.cs]

Thus, the intercepted method writes its `ref` and `out` arguments to the tuple of the generated interceptor, and the
interceptor can read the new values after `proceed` returns. `CallLog.Invoke` uses this to compare the arguments
before and after the call:

[!metalama-file weather-app/CallLog.cs member="CallLog.Invoke"]

An interceptor that does not call `proceed`, such as a fake that returns a canned result, can also assign the elements
of the tuple, and these values are written back to the call site.

## The templates

The templates prepare the tuple in three places.

First, `CreateArgs` creates the tuple from the parameters of the generated method. An `out` parameter has no value
before the call, so its element is the default value of its type:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.CreateArgs"]

Second, `GetInvoker` passes the elements of the tuple to the intercepted method. The local function receives the
tuple as a `ref` parameter. `ExpressionFactory.Parse` creates the expression of this parameter with the tuple type and
with `isReferenceable: true`, so that its elements can be passed as `ref` and `out` arguments:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.GetInvoker"]

Third, after the call, the `Invoke` and `InvokeVoid` templates assign the `ref` and `out` parameters of the generated
method from the tuple. The assignment is in a `finally` block, so it also happens when the intercepted method throws an
exception after it has assigned an argument. The templates add this block only when the method has a `ref` or `out`
parameter:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.Invoke"]

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.WriteBackArguments"]

The templates generate the following interceptors:

```csharp
internal static string[] WeatherClient_GetAlerts_Interceptor(WeatherClient receiver, ref int cursor, int pageSize)
{
    var args = (receiver, cursor, pageSize);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetAlerts", "WeatherClient.GetAlerts(ref int, int)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
    try
    {
        return interceptor.Invoke(ref args, metadata, Proceed);
    }
    finally
    {
        cursor = args.cursor;
    }
    static string[] Proceed(ref (WeatherClient receiver, int cursor, int pageSize) receivedArgs)
    {
        return receivedArgs.receiver.GetAlerts(ref receivedArgs.cursor, receivedArgs.pageSize);
    }
}
internal static bool WeatherClient_TryGetHumidity_Interceptor(WeatherClient receiver, string city, out int humidity)
{
    var args = (receiver, city, humidity: default(int));
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "TryGetHumidity", "WeatherClient.TryGetHumidity(string, out int)", true);
    IUniversalInterceptor interceptor = CallLog.Current.Value;
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

## Async methods with ref or out parameters

The delegate of an async method receives the tuple by value, because an async method cannot have `ref` parameters. So,
the arguments could not be written back. `TryChooseAsyncTemplate` rejects such methods, and
`RedirectToUniversalInterceptor` reports the warning `UNIV05` and leaves the call unchanged. See
<xref:sample-universal-interceptor-2>. C# does not allow `ref` and `out` parameters in an `async` method, so such
methods are rare: they return a task without being `async` themselves.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
