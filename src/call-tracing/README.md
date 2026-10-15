---
uid: sample-call-tracing
summary: "How to trace the calls to a library from one file that you can delete or enable only in some builds, using Metalama interceptors and a project fabric."
keywords: "interceptors, call tracing, temporary instrumentation, investigate without a debugger, project fabric, T# template, Metalama.Extensions.Interceptors"
created-date: 2026-10-12
modified-date: 2026-10-12
---

# Example: tracing calls to investigate a problem

A developer or an AI agent who investigates a problem without a debugger often adds log statements to many files,
and must remove them after the investigation. With interceptors, you can trace the calls to a library from one file:
a project fabric that registers a T# template for each call to the library. The source code of the calling code does
not change. To remove the tracing, you delete the file.

This series shows how to trace the calls to `InventoryClient`, a class of a separate class library. Each call to its
methods is rewritten to call a generated interceptor, which writes the caller, the arguments, the result or the
exception, and the duration of the call:

[!metalama-compare call-tracing-1/OrderService.cs]

## In this series

| Article | Description |
|---------|-------------|
| <xref:sample-call-tracing-1> | A project fabric and a T# template trace each call to the API of a class library. |
| <xref:sample-call-tracing-2> | The fabric registers the interceptors only when an MSBuild property is set. |

> [!div class="see-also"]
> <xref:intercepting-calls>
> <xref:fabrics>
