---
uid: sample-retry-2
level: 200
summary: "The document discusses handling `async` methods in a retry mechanism, using `await Task.Delay` instead of `Thread.Sleep`, and highlights limitations like improper `CancellationToken` handling and basic logging."
keywords: "async methods, retry mechanism, OverrideAsyncMethod, meta.ProceedAsync, meta.Proceed, .NET"
created-date: 2023-04-05
modified-date: 2024-09-09
---

# Retry example, step 2: Handling async methods

[!metalama-project-buttons .]

In the previous example, `async` methods were handled using the same template as that of the normal methods.
Consequently, we used a synchronous call to `Thread.Sleep` instead of an asynchronous `await Task.Delay`, which
essentially negated the `async` nature of the original method.

This new aspect addresses this problem, providing a template that is meant specifically for `async` methods.

[!metalama-compare RemoteCalculator.cs ]

## Implementation

The aspect provides a second template, <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideAsyncMethod>, which
will provide the `async` implementation of the method.

[!metalama-file RetryAttribute.cs ]

The `async` template uses `await meta.ProceedAsync()` instead of `meta.Proceed()`, and `await Task.Delay` instead
of `Thread.Sleep`.

## Limitations

There are still two limitations in this example:

* The aspect does not correctly handle a `CancellationToken`.
* The logging is very basic and is hardcoded to `Console.WriteLine`.

> [!div class="see-also"]
> <xref:simple-override-method>
> <xref:quickstart-adding-aspects>



