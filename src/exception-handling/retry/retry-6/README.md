---
uid: sample-retry-6
level: 300
summary: "This example retries the calls to the idempotent methods of a class library that you do not own, with a project fabric and T# templates that generate interceptors, including for async methods."
keywords: "retry, interceptors, third-party library, code you don't own, idempotent methods, project fabric, T# template, InterceptMethods, RedirectToSynthesizedMethod, MethodTemplateSelector, async methods, Metalama.Extensions.Interceptors"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Retry example, step 6: Retrying calls to code you don't own

[!metalama-project-buttons .]

In steps 1 to 5, the `[Retry]` aspect overrides the method to which it is applied. This requires the source code of
the method. However, the calls that fail for transient reasons are often calls to a library that you do not own, for
instance the client of a remote service. You cannot add an aspect to the methods of this library.

In this example, the application calls `ExchangeRateClient`, a class of a separate class library. The library does not
use Metalama. To simulate transient failures, each method of `ExchangeRateClient` fails the first two times it is
called.

Instead of overriding the methods of `ExchangeRateClient`, Metalama rewrites the calls to these methods in the source
code of the application. Each call calls a generated _interceptor_ method, which retries the call. The calling code
itself does not change in the source code:

[!metalama-compare PriceService.cs]

When the program runs, it produces the following output:

```
The exchange rate service did not respond in time. Waiting 200 ms.
The exchange rate service did not respond in time. Waiting 400 ms.
100 USD = 92.00 EUR
The exchange rate service did not respond in time. Waiting 200 ms.
The exchange rate service did not respond in time. Waiting 400 ms.
100 USD = 79.00 GBP
Order 1: 50 EUR
```

## Implementation

The project references the `Metalama.Extensions.Interceptors` package. The retry logic is implemented by the following
project fabric:

[!metalama-file RetryFabric.cs]

The `AmendProject` method registers the interceptors with the `InterceptMethods` method:

* `Type( typeof(ExchangeRateClient) ).AllMethods()` selects all the methods that `ExchangeRateClient` declares.
  Metalama intercepts the calls to these methods in the source code of the current project.
* `Where` keeps only the methods whose name starts with `Get`. These methods read data, so calling them twice has the
  same effect as calling them once. In other words, they are _idempotent_. `BuyCurrencyAsync` is not idempotent: if
  the first call places the order but fails before it returns, a retry places a second order. Therefore, the calls
  to `BuyCurrencyAsync` are not intercepted.
* `RedirectToSynthesizedMethod` generates the interceptor methods from the templates of the fabric. The
  `MethodTemplateSelector` gives two templates: `Retry` for the methods that are not async, and `RetryAsync` for the
  async methods.

The `Retry` and `RetryAsync` templates have the same code as the templates of the `[Retry]` aspect of step 3, with the
following differences:

* `meta.Proceed()` and `meta.ProceedAsync()` call the intercepted method of `ExchangeRateClient`, with the arguments
  of the call site.
* `RetryAsync` declares a run-time parameter `cancellationToken`, and passes it to `Task.Delay`. Metalama binds the
  run-time parameters of the template to the parameters of the interceptor by name, so `cancellationToken` receives
  the `CancellationToken` argument of the call. For details, see <xref:interceptor-binding-rules>.
* The maximum number of attempts and the delay are properties of the fabric, instead of properties of the aspect.

Metalama generates the interceptor methods in an internal static class named `MetalamaInterceptors`, in the global
namespace. The interceptor of `GetRateAsync` is an `async` method, and it awaits the call to `GetRateAsync` inside the
retry loop.

## Comparison with steps 1 to 5

| | Steps 1 to 5: aspect | Step 6: interceptors |
|---|---|---|
| What changes | The implementation of the method. | The calls to the method, in the current project. |
| Source code of the method | Required. | Not required. The method can be in any assembly. |
| Selection of the methods | You add the `[Retry]` attribute to each method. | A fabric selects the methods with a query, for instance by name. |
| Callers that retry | All the callers of the method. | Only the callers in the projects that register the interceptors. |

Use the aspect of steps 1 to 5 when you own the method and all its callers must retry it. Use interceptors when you do
not own the method, or when only some callers must retry it.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:generating-interceptors-with-templates>
> <xref:interceptors-selecting-calls>
> <xref:fabrics>
