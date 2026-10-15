---
uid: sample-universal-interceptor-1
level: 300
summary: "This example redirects every call to a third-party client class to one run-time object, which receives the receiver and the arguments of the call as a value tuple and logs the call."
keywords: "interceptors, universal interceptor, IUniversalInterceptor, RedirectToUniversalInterceptor, value tuple, ITuple, TypeFactory.CreateTupleType, T# template, project fabric, third-party API"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Universal interceptor example, step 1: Routing all calls to one object

[!metalama-project-buttons .]

In this step, a project fabric redirects every call to `WeatherClient` to a `CallLog` object. `CallLog` writes the
method, the arguments and the result of each call. It is plain C# code, and it does not depend on the signatures of
the methods of `WeatherClient`.

Metalama rewrites each call to `WeatherClient` so that it calls a generated interceptor method. The calling code does
not change in the source code:

[!metalama-compare WeatherReport.cs]

When the program runs, `CallLog` writes the following lines:

```
[call] WeatherClient.SetUnit(Celsius) succeeded.
[call] WeatherClient.GetTemperature(Prague) returned 12.5.
Prague: 12.5 Celsius
[call] WeatherClient.SetUnit(Fahrenheit) succeeded.
[call] WeatherClient.GetTemperature(Lisbon) returned 69.8.
Lisbon: 69.8 Fahrenheit
[call] WeatherClient.SetUnit(Celsius) succeeded.
[call] WeatherClient.GetTemperature(Atlantis) threw ArgumentException: Unknown city 'Atlantis'. (Parameter 'city')
```

## The run-time interface

The `interceptor-library` project defines the interface that the run-time object implements. Its design follows the
`IInterceptor` interface of the static proxy example:

[!metalama-file ../interceptor-library/IUniversalInterceptor.cs]

`TArgs` is a value tuple that contains the receiver of the call, when the intercepted method is not static, and the
arguments. For a call to `client.GetTemperature( "Prague" )`, `TArgs` is `(WeatherClient receiver, string city)`.
The constraint `ITuple` lets the run-time code read the elements of any tuple by index.

The delegate calls the intercepted method with the elements of the tuple. It receives the tuple by reference, so the
intercepted method can write its `ref` and `out` arguments to the tuple. See <xref:sample-universal-interceptor-2>.

[!metalama-file ../interceptor-library/InterceptorDelegate.cs]

`InterceptionMetadata` describes the intercepted method:

[!metalama-file ../interceptor-library/InterceptionMetadata.cs]

## The logger

`CallLog` implements `IUniversalInterceptor`. Its `Invoke` method writes the call, calls `proceed`, and writes the
result or the exception. A void method has no result, so the generated code uses `ValueTuple` as `TResult` for it.
In this step, the asynchronous methods are not logged.

The static field `Current` holds the instance that receives the calls.

[!metalama-file CallLog.cs]

## The fabric

The following project fabric redirects every call to the methods of `WeatherClient` to `CallLog.Current`:

[!metalama-file InterceptionFabric.cs]

`RedirectToUniversalInterceptor` is an extension method of `IMethodResultFactory`, which the `interceptor-library`
project defines. Its first argument is a type that implements `IUniversalInterceptor`, and the second argument is the
name of a static field or property of this type, which gives the instance.

## Generating the interceptors

`RedirectToUniversalInterceptor` is compile-time code:

[!metalama-file ../interceptor-library/UniversalInterceptorExtensions.cs]

`IMethodResultFactory` is the base interface of the method selection and of the call site in `ForEachSite`, so the
method is available in both places. On the method selection, it calls `ForEachSite` itself, because the template and
its arguments depend on the intercepted method of each call site.

For each call site, the method does the following:

1. It checks the member that gives the interceptor. When the type does not implement `IUniversalInterceptor`, or when
   the member does not exist, is not static, or has another type, it reports the errors `UNIV01` to `UNIV04` and
   leaves the call unchanged. The test `InvalidInterceptorMember` of the
   `Metalama.Samples.UniversalInterceptor1.Tests` project shows these errors.
2. It creates the type of the tuple with `TypeFactory.CreateTupleType`. The elements have the names of the parameters
   of the generated method: `receiver`, then the parameters of the intercepted method.
3. It chooses the template from the return type of the intercepted method, and calls `RedirectToSynthesizedMethod`
   with the template, the tuple type and the result type.

The templates are in the `UniversalInterceptorTemplates` class:

[!metalama-file ../interceptor-library/UniversalInterceptorTemplates.cs]

For instance, the `Invoke` template generates the following interceptor for `WeatherClient.GetTemperature`:

```csharp
internal static double WeatherClient_GetTemperature_Interceptor(WeatherClient receiver, string city)
{
    var args = (receiver, city);
    var metadata = new InterceptionMetadata(typeof(WeatherClient), "GetTemperature", "WeatherClient.GetTemperature(string)", true);
    IUniversalInterceptor interceptor = CallLog.Current;
    return (double)interceptor.Invoke(ref args, metadata, Proceed);

    static double Proceed(ref (WeatherClient receiver, string city) receivedArgs)
    {
        return receivedArgs.receiver.GetTemperature(receivedArgs.city);
    }
}
```

The local function `Proceed` is static: it reads the receiver and the arguments from the tuple, not from the
parameters of the generated method. The C# compiler therefore caches the delegate, and the call does not allocate
it.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
> <xref:interceptors-selecting-calls>
