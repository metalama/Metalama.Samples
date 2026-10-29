---
uid: sample-universal-interceptor-4
level: 300
summary: "This example completes the universal interceptor: it validates the member that gives the interceptor, skips the calls on struct receivers with ForEachSite, and stores the interceptor in an AsyncLocal so that each xUnit test installs its own fake."
keywords: "interceptors, universal interceptor, RedirectToUniversalInterceptor, diagnostics, struct receiver, ForEachSite, Skip, AsyncLocal, fake, test double, xUnit, parallel tests"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Universal interceptor example, step 4: Putting it all together

[!metalama-project-buttons weather-app]

The previous articles explained how a call to a synchronous method, to an async method, and to a method with `ref` or
`out` parameters reaches the interceptor. This article explains the rest of the project:

* how `RedirectToUniversalInterceptor` validates the member that gives the interceptor;
* why the calls on a struct receiver are skipped;
* how the interceptor is stored in an `AsyncLocal<T>`, so that tests replace it with a fake.

When the program runs, it writes the following lines:

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
[call] WeatherClient.RefreshAsync(CancellationToken) succeeded asynchronously.
[call] WeatherClient.ClearCacheAsync(CancellationToken) succeeded asynchronously.
[call] WeatherClient.GetForecastAsync(Prague, 3, CancellationToken) returned Forecast { City = Prague, Days = 3, AverageHigh = 13.5 } asynchronously.
[call] WeatherClient.GetTemperatureAsync(Prague, CancellationToken) returned 12.5 asynchronously.
Prague: 12.5 now, 13.5 on average in the next 3 days
[call] WeatherClient.TryGetHumidity(Prague, 0) returned True, and changed the arguments to (Prague, 71).
Prague: humidity 71 %
[call] WeatherClient.TryGetHumidity(Lisbon, 0) returned False.
Lisbon: humidity unknown
[call] WeatherClient.GetAlerts(0, 2) returned [Lisbon: strong wind on Monday., Prague: frost on Tuesday.], and changed the arguments to (2, 2).
[call] WeatherClient.GetAlerts(2, 2) returned [Prague: heavy rain on Thursday.], and changed the arguments to (3, 2).
[call] WeatherClient.GetAlerts(3, 2) returned [].
Alerts: Lisbon: strong wind on Monday. Prague: frost on Tuesday. Prague: heavy rain on Thursday.
[call] WeatherClient.GetQuota() returned 2 requests remaining.
Available requests: 2
```

## The RedirectToUniversalInterceptor method

`RedirectToUniversalInterceptor` is an extension method of `IMethodResultFactory`. This interface is the base
interface of the method selection and of the call site in `ForEachSite`, so the method is available in both places,
like `RedirectToSynthesizedMethod` and `RedirectToExistingMethod`. On the method selection, the method calls
`ForEachSite` itself, because the template and its arguments depend on the intercepted method of each call site:

[!metalama-file weather-app/UniversalInterceptorExtensions.cs member="UniversalInterceptorExtensions.RedirectToUniversalInterceptor"]

For each call site, `RedirectSite` validates the member that gives the interceptor, skips the calls that cannot be
intercepted, and chooses the template as explained in the previous articles:

[!metalama-file weather-app/UniversalInterceptorExtensions.cs member="UniversalInterceptorExtensions.RedirectSite"]

## Validating the interceptor member

The arguments of `RedirectToUniversalInterceptor` are a type and the name of a member. A mistake in these arguments
must be reported at build time, not as an exception at run time. `TryGetInterceptorMember` checks that the type
implements `IUniversalInterceptor`, and that the member exists, is static, and has the type `IUniversalInterceptor` or
`AsyncLocal<IUniversalInterceptor>`:

[!metalama-file weather-app/UniversalInterceptorExtensions.cs member="UniversalInterceptorExtensions.TryGetInterceptorMember"]

The errors are defined in the `DiagnosticDefinitions` class:

[!metalama-file weather-app/DiagnosticDefinitions.cs]

`IMethodResultFactory` gives no access to a diagnostic sink, so the errors are reported at each call site, through
`site.Diagnostics`. A registration with an invalid member and no call site reports no error.

The following test registers four invalid members:

[!metalama-file ../tests/Metalama.Samples.UniversalInterceptor.Tests/InvalidInterceptorMember/InvalidInterceptorMember.cs]

The test reports the following errors, on the four calls to the `Run` methods:

```
UNIV01: The type 'NotAnInterceptor' cannot give the universal interceptor because it does not implement IUniversalInterceptor.
UNIV02: The type 'Interceptor' has no field or property named 'Missing'.
UNIV03: The member 'Interceptor.InstanceMember' cannot give the universal interceptor because it is not static.
UNIV04: The member 'Interceptor.WrongType' cannot give the universal interceptor because its type is 'Interceptor'. The type must be IUniversalInterceptor or AsyncLocal<IUniversalInterceptor>.
```

## Skipping the calls on struct receivers

The tuple contains a copy of the receiver. When the receiver is a struct, the intercepted method would change the copy,
and the changes would be lost. For instance, `RequestQuota` is a mutable struct, and its `TryConsume` method decrements
the number of remaining requests:

[!code-csharp[](weather-client/RequestQuota.cs)]

The `CountAvailableRequests` method of `WeatherReport` calls `TryConsume` in a loop. If the call were intercepted,
`quota` would never change, and the loop would never end:

[!metalama-file weather-app/WeatherReport.cs marker="StructReceiver"]

So, `RedirectToUniversalInterceptor` does not intercept a call on a struct receiver. It reports the warning `UNIV05`
and leaves the call unchanged, as the following test shows:

[!metalama-file ../tests/Metalama.Samples.UniversalInterceptor.Tests/StructReceiverWarning/StructReceiverWarning.cs]

[!metalama-file ../tests/Metalama.Samples.UniversalInterceptor.Tests/StructReceiverWarning/StructReceiverWarning.t.cs]

The project fabric of the application skips these calls itself, with `ForEachSite` and `site.Skip()`, so the warning
is not reported. It redirects the other calls with `site.RedirectToUniversalInterceptor`:

[!metalama-file weather-app/InterceptionFabric.cs]

The receiver of an extension method is an ordinary argument of the static method, so this rule does not apply to it.

## Replacing the interceptor in tests

Code that calls a third-party API directly is hard to test: the tests call the real service, and they cannot choose
its results. With a universal interceptor, a test can install a fake interceptor that returns canned results, without
changing the code under test.

A static field of type `IUniversalInterceptor` is shared by all the tests. When the tests run in parallel, a test would
replace the interceptor of the other tests. So, `CallLog.Current` is an `AsyncLocal<IUniversalInterceptor>`, and each
test installs the interceptor for its own asynchronous flow:

[!metalama-file weather-app/CallLog.cs member="CallLog.Current"]

`RedirectToUniversalInterceptor` accepts a member of this type. The templates then read the `Value` property:

[!metalama-file weather-app/UniversalInterceptorTemplates.cs member="UniversalInterceptorTemplates.GetInterceptor"]

The generated code thus contains the following line:

```csharp
IUniversalInterceptor interceptor = CallLog.Current.Value;
```

The `Value` property returns the value assigned in the current asynchronous flow, or in the flow that started it. A
value assigned in a flow is not visible in the other flows. The value must be assigned before the first intercepted
call, otherwise the call throws a `NullReferenceException`. The program assigns it at the start:

[!metalama-file weather-app/Program.cs]

The tests are in the `Metalama.Samples.UniversalInterceptor.UnitTests` project, which references the application
project. The calls in `WeatherReport` are intercepted when the application project is built, so the tests call
`WeatherReport` as an ordinary class.

`FakeWeatherInterceptor` returns canned results, by method name, and records the intercepted calls. The calls to a
method without a canned result proceed to `WeatherClient`:

[!metalama-file ../tests/Metalama.Samples.UniversalInterceptor.UnitTests/FakeWeatherInterceptor.cs]

`InterceptorScope.RunAsync` runs the code of a test in a new asynchronous flow, with `Task.Run`, and installs the fake
in this flow. The assignment is not visible in the flow of the test runner, so it does not affect the other tests:

[!metalama-file ../tests/Metalama.Samples.UniversalInterceptor.UnitTests/InterceptorScope.cs]

xUnit runs the test classes in parallel. Each test installs a fake with different results:

[!metalama-file ../tests/Metalama.Samples.UniversalInterceptor.UnitTests/WeatherReportTests.cs]

The `IsolationTests` class shows that two flows can use different interceptors at the same time. Each flow installs
its fake, then waits until the other flow has installed its fake, and only then calls `WeatherReport`. Both calls
reach the fake of their own flow.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:interceptors-selecting-calls>
> <xref:redirecting-calls-to-existing-methods>
