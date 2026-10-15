---
uid: sample-call-tracing-1
level: 300
summary: "This example traces each call to the API of a class library with a project fabric and a T# template interceptor: the caller, the arguments, the result, the exception and the duration."
keywords: "interceptors, call tracing, temporary instrumentation, project fabric, T# template, InterceptMethods, RedirectToSynthesizedMethod, Metalama.Extensions.Interceptors"
created-date: 2026-10-12
modified-date: 2026-10-12
---

# Call tracing example, step 1: Tracing calls to a library

[!metalama-project-buttons .]

When you investigate a problem without a debugger, for instance in a CI build, you need to know which calls the
application makes to a library, with which arguments, and what each call returns. Adding log statements around each
call changes many files, and you must remove these statements after the investigation.

In this example, the application calls `InventoryClient`, a class of a separate class library. The library does not
use Metalama, and its source code does not need to be available. One file of the application, `TracingFabric.cs`,
traces every call to the methods of `InventoryClient`.

Metalama rewrites each call to `InventoryClient` so that it calls a generated interceptor method. The calling code
itself does not change in the source code:

[!metalama-compare OrderService.cs]

When the program runs, the interceptors write the following trace. The durations change from one run to another.

```
[trace] OrderService.CanOrder -> InventoryClient.GetStock(sku: BOOK) returned 10 in 1.198 ms.
[trace] OrderService.PlaceOrder -> InventoryClient.Reserve(sku: BOOK, quantity: 3) succeeded in 0.261 ms.
[trace] OrderService.PlaceOrder -> InventoryClient.Reserve(sku: PEN, quantity: 5) threw InvalidOperationException in 0.741 ms: Only 2 items of 'PEN' are in stock.
```

## Implementation

The project references the `Metalama.Extensions.Interceptors` package. The tracing is implemented by the following
project fabric:

[!metalama-file TracingFabric.cs]

The `AmendProject` method registers the interceptors with the `InterceptMethods` method:

* `Type( typeof(InventoryClient) ).AllMethods()` selects all the methods that `InventoryClient` declares. Metalama
  intercepts the calls to these methods in the source code of the current project.
* `RedirectToSynthesizedMethod( nameof(this.Trace) )` generates the interceptor methods from the `Trace` template.
  The template is a member of the fabric, so the registration does not need to give a template provider.
* `WithGranularity( InterceptorGranularity.Member )` generates one interceptor for each calling member. Without this
  call, the call sites in different members share an interceptor, and the template cannot read the calling member.

The `Trace` template is a T# template, like the templates of override aspects. It reads the description of the
interception with `meta.Interception`. This extension property requires C# 14. With an earlier version of C#, use
`meta.GetExtension<IInterceptionInfo>()` instead.

* `CallingType` and `CallingMember` give the caller.
* `Method.InterceptedMethod` gives the method that the call site calls.
* `Method.ArgumentParameters` gives the parameters of the interceptor that receive the arguments of the call.
* `meta.Proceed()` calls the intercepted method with the arguments of the call site.

The template measures the duration with a `Stopwatch`, and writes the result or the exception. Because
`Reserve` returns `void`, the template checks the return type of the intercepted method at compile time and writes a
different message for `void` methods.

Metalama generates the interceptor methods in an internal static class named `MetalamaInterceptors`, in the global
namespace.

## Removing the tracing

The tracing exists only in `TracingFabric.cs`. To remove it, delete this file. The calling code and the library do not
change. To keep the file but enable the tracing only in some builds, see <xref:sample-call-tracing-2>.

> [!div class="see-also"]
> <xref:intercepting-calls>
> <xref:generating-interceptors-with-templates>
> <xref:fabrics>
