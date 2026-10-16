---
uid: sample-universal-interceptor
summary: "How to route the calls to many methods of a third-party API, with different signatures, through one run-time object, using Metalama interceptors that pack the arguments into a value tuple."
keywords: "interceptors, universal interceptor, third-party API, value tuple, ITuple, AsyncLocal, fake, unit tests, RedirectToUniversalInterceptor, Metalama.Extensions.Interceptors"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Example: one universal interceptor for all calls to a third-party API

A team that adds logging, retry or auditing around a whole third-party API wants to write this behavior once, in plain
C#. In the same way, tests often need to replace the behavior of the API with canned results. Both needs require that
every call to the API, whatever its signature, goes through one run-time object.

In this example, the application calls `WeatherClient`, a class of a separate class library whose source code cannot
be changed. The class has synchronous and asynchronous methods, and methods with `ref` and `out` parameters. A project
fabric redirects every call to `WeatherClient` to a _universal interceptor_: an object that implements the
`IUniversalInterceptor` interface. The calling code does not change in the source code.

[!metalama-project-buttons weather-app]

## How it works

Metalama rewrites each call site so that it calls a generated interceptor method. The generated method does three
things:

1. It packs the receiver of the call, which is the object on which the method is called, and the arguments into a value
   tuple.
2. It reads the `IUniversalInterceptor` instance from a static field or property.
3. It calls the `Invoke` or `InvokeAsync` generic method of this instance. It passes the tuple, an
   `InterceptionMetadata` value that describes the intercepted method, and a delegate that reads the tuple and calls
   the intercepted method.

The run-time object receives the tuple as a type parameter `TArgs`, constrained to `ITuple`. Its code does not depend
on the signature of the intercepted method, so one class handles every method of the API.

## In this series

The example is one project, `weather-app`. It contains the run-time interface, the T# templates of the generated
methods, the `RedirectToUniversalInterceptor` method, the `CallLog` interceptor and the application. The articles of
this series explain the project one part at a time, from the simplest calls to the complete solution.

| Article | Description |
|---------|-------------|
| <xref:sample-universal-interceptor-1> | Redirects the calls to synchronous methods to `CallLog`, which writes the method, the arguments and the result. |
| <xref:sample-universal-interceptor-2> | Adds the methods that return a `Task`, a `Task<T>`, a `ValueTask` or a `ValueTask<T>`. |
| <xref:sample-universal-interceptor-3> | Adds the methods with `ref` and `out` parameters, whose arguments are written back to the call site. |
| <xref:sample-universal-interceptor-4> | Validates the member that gives the interceptor, skips the calls on struct receivers, and stores the interceptor in an `AsyncLocal<T>`, so that each xUnit test installs its own fake. |

## Comparison with the other interceptor examples

The other examples of interceptors, <xref:sample-call-tracing> and <xref:sample-call-telemetry>, generate the behavior
from a T# template. The static proxy example of this repository, in `src/Metalama.Samples.Proxy`, uses a run-time
interface `IInterceptor` like this example, but it intercepts the members of an interface, not call sites.

| | Per-method templates | Universal interceptor | Static proxy |
|---|---|---|---|
| What is intercepted | The call sites of the project. | The call sites of the project. | The calls made through a proxy object, which implements an interface of the project. |
| Where the behavior is written | In a T# template. Each generated interceptor contains the expanded behavior. | In a run-time class. The generated interceptors only pack the arguments and call this class. | In a run-time class, like the universal interceptor. |
| Changing the behavior | Change the template and rebuild. | Assign another object to the static member, at run time. | Pass another object to the constructor of the proxy. |
| Calling code | Unchanged. | Unchanged. | Must create the proxy and call it instead of the original object. |
| Testing the behavior | Through the transformed code. | With ordinary unit tests of the run-time class. | With ordinary unit tests of the run-time class. |
| Knowledge of the call | The template reads the calling code and the types at compile time. | The run-time class receives the tuple and the metadata, and reads the arguments as `object`. | The same as the universal interceptor. |

Choose per-method templates when the behavior is fixed at build time and must be fast, for instance tracing or timing.
Choose a universal interceptor when the behavior must change at run time, or when you want to write and test it as
plain C#. Choose a static proxy when you own the interface and you want an object that you can pass to other code.

## Limitations

The universal interceptor has these limitations:

* Calls on a struct receiver are not intercepted. The tuple would contain a copy of the receiver, so the changes that
  the method makes to the receiver would be lost. `RedirectToUniversalInterceptor` leaves these calls unchanged and
  reports the warning `UNIV05`. Skip them in `ForEachSite` to remove the warning. See
  <xref:sample-universal-interceptor-4>. The receiver of an extension method is an ordinary argument, so this rule
  does not apply to it.
* Methods that return an awaitable type and have `ref` or `out` parameters are not intercepted. The delegate of an
  async method receives the tuple by value, so the arguments could not be written back.
* Methods that return an awaitable type other than `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>` are not
  intercepted.
* Method groups converted to delegates, such as `cities.Select( client.GetTemperature )`, are not intercepted, because
  interceptors generated from templates support only calls.

Each call has a cost at run time:

* The receiver and the arguments are copied into the tuple. The `ref` and `out` arguments are copied back after the
  call.
* `Invoke` and `InvokeAsync` are generic methods of an interface, so each call is a generic virtual method call, which
  is slower than a non-generic interface call.
* The interceptor reads the elements of the tuple through `ITuple`, whose indexer returns `object`, so it boxes the
  elements of a value type.
* The calls of async methods allocate the state machines of the generated method and of `InvokeAsync`, when the
  operation does not complete synchronously.

The delegate does not cause an allocation for each call: it is created from a static local function, and the C#
compiler caches such delegates in a static field. The `InterceptionMetadata` value is a struct, so it is not allocated
on the heap.

> [!WARNING]
> Interceptors only rewrite the calls that are in the source code of the current project. They do not see the calls
> made inside the library, by the framework, or through reflection.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
> <xref:interceptors-selecting-calls>
