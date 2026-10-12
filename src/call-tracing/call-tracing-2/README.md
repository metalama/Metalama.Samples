---
uid: sample-call-tracing-2
level: 300
summary: "This example registers the call-tracing interceptors only when an MSBuild property is set, so that the tracing exists only in a trace build."
keywords: "interceptors, call tracing, trace build, MSBuild property, TryGetProperty, CompilerVisibleProperty, project fabric, Metalama.Extensions.Interceptors"
created-date: 2026-10-12
modified-date: 2026-10-12
---

# Call tracing example, step 2: Enabling tracing only in a trace build

[!metalama-project-buttons .]

In the previous step, the tracing applied to every build until you deleted the fabric. Sometimes you want to keep
the fabric in the source code, for instance to investigate the same problem again later, but you do not want the
tracing in the builds that you release.

In this step, the fabric registers the interceptors only when the `TraceCalls` MSBuild property is `True`. In any
other build, the fabric does nothing, and the calls to `InventoryClient` are not rewritten.

## Enabling the tracing

To build and run the program with the tracing, set the property on the command line:

```
dotnet run --property:TraceCalls=True
```

The program then writes the same trace as in the previous step:

```
[trace] OrderService.CanOrder -> InventoryClient.GetStock(sku: BOOK) returned 10 in 1.371 ms.
[trace] OrderService.PlaceOrder -> InventoryClient.Reserve(sku: BOOK, quantity: 3) succeeded in 0.242 ms.
[trace] OrderService.PlaceOrder -> InventoryClient.Reserve(sku: PEN, quantity: 5) threw InvalidOperationException in 0.600 ms: Only 2 items of 'PEN' are in stock.
```

When you run `dotnet run` without the property, the program writes nothing.

## Implementation

The fabric reads the MSBuild property with the `TryGetProperty` method of `amender.Project`, and returns before it
registers the interceptors when the property is missing or is not `True`:

[!metalama-file TracingFabric.cs]

A fabric can read only the MSBuild properties that the project declares as `CompilerVisibleProperty` items. The
project file therefore declares the `TraceCalls` property:

```xml
<ItemGroup>
    <CompilerVisibleProperty Include="TraceCalls"/>
</ItemGroup>
```

The template is the same as in the previous step.

Instead of an MSBuild property, the fabric could test a preprocessor symbol with `amender.Project.PreprocessorSymbols`,
or the build configuration with `amender.Project.Configuration`.

> [!div class="see-also"]
> <xref:intercepting-calls>
> <xref:interceptors-selecting-calls>
> <xref:reading-msbuild-properties>
