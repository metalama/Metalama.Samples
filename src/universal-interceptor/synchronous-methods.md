---
uid: sample-universal-interceptor-1
level: 300
summary: "This example redirects every call to the synchronous methods of a third-party client class to one run-time object, which receives the receiver and the arguments of the call as a value tuple and logs the call."
keywords: "interceptors, universal interceptor, IUniversalInterceptor, RedirectToUniversalInterceptor, value tuple, ITuple, TypeFactory.CreateTupleType, T# template, project fabric, third-party API"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Universal interceptor example, step 1: Routing synchronous calls to one object

[!metalama-project-buttons weather-app]

This article explains how the calls to the synchronous methods of `WeatherClient` reach one run-time object,
`CallLog`. `CallLog` writes the method, the arguments and the result of each call. It is plain C# code, and it does
not depend on the signatures of the methods of `WeatherClient`.

The `Describe` method of `WeatherReport` calls a void method, `SetUnit`, and a method that returns a value,
`GetTemperature`:

[!metalama-file weather-app/WeatherReport.cs marker="SynchronousCalls"]

Metalama rewrites both calls so that they call generated interceptor methods. The following test shows the
transformation:

[!metalama-compare ../tests/Metalama.Samples.UniversalInterceptor.Tests/SynchronousCalls.cs]

When the program calls `Describe`, `CallLog` writes the following lines:

```
[call] WeatherClient.SetUnit(Celsius) succeeded.
[call] WeatherClient.GetTemperature(Prague) returned 12.5.
Prague: 12.5 Celsius
[call] WeatherClient.SetUnit(Fahrenheit) succeeded.
[call] WeatherClient.GetTemperature(Lisbon) returned 69.8.
Lisbon: 69.8 Fahrenheit
[call] WeatherClient.SetUnit(Celsius) succeeded.
[call] WeatherClient.GetTemperature(Atlantis) threw ArgumentException: Unknown city 'Atlantis'. (Parameter 'city')
Atlantis: unknown city
```

## The run-time interface

The run-time object implements the `IUniversalInterceptor` interface. Its design follows the `IInterceptor` interface
of the static proxy example. A synchronous call goes to the `Invoke` method:

[!metalama-file weather-app/IUniversalInterceptor.cs marker="Invoke"]

`TArgs` is a value tuple that contains the receiver of the call, when the intercepted method is not static, and the
arguments. For a call to `client.GetTemperature( "Prague" )`, `TArgs` is `(WeatherClient receiver, string city)`.
The constraint `ITuple` lets the run-time code read the elements of any tuple by index.

A void method has no result, so the generated code uses `ValueTuple` as `TResult` for it. One `Invoke` method thus
handles the methods with and without a return value.

The delegate calls the intercepted method with the elements of the tuple. It receives the tuple by reference, so that
the intercepted method can write its `ref` and `out` arguments to the tuple. See
<xref:sample-universal-interceptor-3>.

[!metalama-file weather-app/InterceptorDelegate.cs]

`InterceptionMetadata` describes the intercepted method:

[!metalama-file weather-app/InterceptionMetadata.cs]

## The logger

`CallLog` implements `IUniversalInterceptor`. Its `Invoke` method writes the call, calls `proceed`, and writes the
result or the exception. It reads the arguments from the tuple through `ITuple`:

[!metalama-file weather-app/CallLog.cs member="CallLog.Invoke"]

[!metalama-file weather-app/CallLog.cs member="CallLog.FormatArguments"]

## The fabric

The following project fabric redirects every call to the methods of `WeatherClient` to the interceptor that the
`CallLog.Current` member gives:

[!metalama-file weather-app/InterceptionFabric.cs]

`RedirectToUniversalInterceptor` is a compile-time extension method of `IMethodResultFactory`, which this project
defines. Its first argument is a type that implements `IUniversalInterceptor`, and the second argument is the name of
a static field or property of this type, which gives the instance. The `ForEachSite` call, the `RequestQuota` type and
the type of `CallLog.Current` are explained in <xref:sample-universal-interceptor-4>.

## Generating the interceptors

For each call site, `RedirectToUniversalInterceptor` chooses a T# template from the return type of the intercepted
method. A synchronous method gets the `Invoke` template, or the `InvokeVoid` template when it returns `void`:

[!metalama-file weather-app/UniversalInterceptorExtensions.cs member="UniversalInterceptorExtensions.ChooseSynchronousTemplate"]

Then it calls `RedirectToSynthesizedMethod` with the template. The template receives the type of the tuple and the
result type as compile-time type parameters:

[!metalama-file weather-app/UniversalInterceptorExtensions.cs member="UniversalInterceptorExtensions.Redirect"]

The type of the tuple is created with `TypeFactory.CreateTupleType`. Its elements have the names of the parameters of
the generated method: `receiver`, then the parameters of the intercepted method. For a call to an extension method in
reduced form, such as `text.Wrap( ">" )`, the receiver is the first parameter of the static method:

[!metalama-file weather-app/UniversalInterceptorExtensions.cs member="UniversalInterceptorExtensions.CreateArgsType"]

The `Invoke` template creates the tuple, the metadata and the interceptor, and passes the tuple to `Invoke` with a local
function that calls the intercepted method. The branch for `ref` and `out` parameters is explained in
<xref:sample-universal-interceptor-3>.

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.Invoke"]

The `InvokeVoid` template does the same for a void method. Its local function returns `default( ValueTuple )`:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.InvokeVoid"]

The templates call compile-time helper methods. `CreateArgs` creates the tuple from the parameters of the generated
method, `CreateMetadata` creates the `InterceptionMetadata` value, and `GetInvoker` returns the intercepted method,
bound to the receiver that it reads from the tuple:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.CreateArgs"]

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.CreateMetadata"]

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.GetInvoker"]

The templates generate the following interceptors for `GetTemperature` and `SetUnit`:

GENERATED_SYNC

The local function `Proceed` is static: it reads the receiver and the arguments from the tuple, not from the
parameters of the generated method. The C# compiler therefore caches the delegate, and the call does not allocate
it.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
> <xref:interceptors-selecting-calls>
