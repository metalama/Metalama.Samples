---
uid: sample-universal-interceptor-3
level: 300
summary: "This example holds the universal interceptor in an AsyncLocal, so that each xUnit test installs its own fake interceptor that returns canned results, and the tests run in parallel."
keywords: "interceptors, universal interceptor, AsyncLocal, fake, test double, xUnit, parallel tests, canned results, third-party API"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Universal interceptor example, step 3: Replacing the interceptor per test

[!metalama-project-buttons .]

Code that calls a third-party API directly is hard to test: the tests call the real service, and they cannot choose
its results. With a universal interceptor, a test can install a fake interceptor that returns canned results, without
changing the code under test.

A static field such as the `CallLog.Current` field of the previous steps is shared by all the tests. When the tests run
in parallel, a test would replace the interceptor of the other tests. In this step, the field is an
`AsyncLocal<IUniversalInterceptor>`, so each test installs the interceptor for its own asynchronous flow.

## The interceptor member

`CallLog.Current` is now an `AsyncLocal<IUniversalInterceptor>`. The rest of `CallLog` does not change:

[!metalama-file CallLog.cs]

`RedirectToUniversalInterceptor` accepts a member of this type. The generated code then reads the `Value` property:

```csharp
IUniversalInterceptor interceptor = CallLog.Current.Value;
```

The `Value` property returns the value assigned in the current asynchronous flow, or in the flow that started it. A
value assigned in a flow is not visible in the other flows. The value must be assigned before the first intercepted
call, otherwise the call throws a `NullReferenceException`. The program assigns it at the start:

[!metalama-file Program.cs]

When the program runs, it writes the following lines:

```
[call] WeatherClient.GetTemperature(Prague) returned 12.5.
[call] WeatherClient.TryGetHumidity(Prague, 0) returned True, and changed the arguments to (Prague, 71).
Prague: 12.5 Celsius, humidity 71 %
[call] WeatherClient.GetTemperature(Atlantis) threw ArgumentException: Unknown city 'Atlantis'. (Parameter 'city')
Atlantis: unknown city
[call] WeatherClient.GetForecastAsync(Lisbon, 5, CancellationToken) returned Forecast { City = Lisbon, Days = 5, AverageHigh = 22 } asynchronously.
Lisbon: 22 on average in the next 5 days
```

The code under test is `WeatherReport`, which calls `WeatherClient`:

[!metalama-compare WeatherReport.cs]

## The tests

The tests are in the `Metalama.Samples.UniversalInterceptor3.UnitTests` project, which references this project. The
calls in `WeatherReport` are intercepted when this project is built, so the tests call `WeatherReport` as an ordinary
class.

`FakeWeatherInterceptor` returns canned results, by method name, and records the intercepted calls. The calls to a
method without a canned result proceed to `WeatherClient`:

[!metalama-file ../../tests/Metalama.Samples.UniversalInterceptor3.UnitTests/FakeWeatherInterceptor.cs]

`InterceptorScope.RunAsync` runs the code of a test in a new asynchronous flow, with `Task.Run`, and installs the fake
in this flow. The assignment is not visible in the flow of the test runner, so it does not affect the other tests:

[!metalama-file ../../tests/Metalama.Samples.UniversalInterceptor3.UnitTests/InterceptorScope.cs]

xUnit runs the test classes in parallel. Each test installs a fake with different results:

[!metalama-file ../../tests/Metalama.Samples.UniversalInterceptor3.UnitTests/WeatherReportTests.cs]

The `IsolationTests` class proves that two flows can use different interceptors at the same time. Each flow installs
its fake, then waits until the other flow has installed its fake, and only then calls `WeatherReport`. Both calls
reach the fake of their own flow.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:redirecting-calls-to-existing-methods>
