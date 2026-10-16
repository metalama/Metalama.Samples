---
uid: sample-metrics
browse-source:
  - path: .
    children:
      - title: Tests
        path: ../Metalama.Samples.Metrics.Tests
level: 400
summary: "This example builds a family of aspects that instrument methods with System.Diagnostics.Metrics counters, generating a companion metrics class per instrumented type and its dependency injection registration."
keywords: "System.Diagnostics.Metrics, C# metrics, instrumentation, Counter, IMeterFactory, OpenTelemetry, aspect"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Instrumenting methods with metrics

[!metalama-project-buttons .]

<xref:System.Diagnostics.Metrics> is the standard instrumentation interface of .NET. An application creates a <xref:System.Diagnostics.Metrics.Meter>, creates instruments such as <xref:System.Diagnostics.Metrics.Counter`1> from that meter, and updates the instruments from its own code. Collection agents, including OpenTelemetry exporters, then read those instruments.

Instrumenting a method by hand means declaring a field for each instrument, creating the instrument in a constructor, naming it consistently, and updating it in the method body. For a type with several instrumented methods and several measurements per method, most of that code is repetitive.

This example replaces the repetitive part with three method-level aspects. The following code shows a method that counts its calls, measures its execution time and counts the exceptions it throws, and the code that the aspects generate from it.

[!metalama-compare HatShop.cs]

The aspects generate a second type as well, named after the instrumented type, which holds the instruments and obtains the meter through dependency injection.

[!metalama-file Metrics.HatShopMetrics.cs transformed="true"]

## Architecture of the example

The example is built from two kinds of aspects that cooperate.

| Component | Role |
|-----------|------|
| `MetricAttribute` and its three derived classes | Method-level aspects that declare what to measure and supply the templates that measure it. |
| `ImplementMetricsAspect` | A type-level aspect that generates the companion metrics class and overrides the instrumented methods. |
| `GenerateAddMetricsExtensionAttribute` | An assembly-level aspect that generates the dependency injection registration of every generated metrics class. |

The division of work matters. A method-level aspect sees one method, so it cannot create a type that gathers the instruments of all methods. A type-level aspect sees the whole type, but nothing tells it which measurements were requested. The two are connected by the aspect requirement mechanism described in <xref:child-aspects>.

## Step 1. Declaring a measurement

The base class of the three metric aspects adds no advice of its own. Its `BuildAspect` method only requires the type-level aspect on the declaring type.

[!metalama-file MetricAttribute.cs]

The class declares two templates and two compile-time properties, but it never applies them. It acts as a template provider: `ImplementMetricsAspect` reads `MetricKind` and `MetricType` to name and type the instrument, then uses `OverrideMethodTemplate` to override the method and `CreateMetricTemplate` to create the instrument.

The three derived classes supply those templates. Counting the calls of a method takes one statement before the original implementation.

[!metalama-file MeasureExecutionCountAttribute.cs]

`metricProperty.WithObject( metricsField ).WithOptions( InvokerOptions.NullConditional )` builds the expression that reads the instrument from the companion object. <xref:Metalama.Framework.Code.Invokers.InvokerOptions.NullConditional> makes the generated expression use the `?.` operator, because the companion object is an optional dependency and can be null.

Measuring execution time requires a `try` and `finally` block around the original implementation.

[!metalama-file MeasureExecutionTimeAttribute.cs]

The measured value is the difference between two <xref:System.Diagnostics.Stopwatch.GetTimestamp*> readings, so the counter accumulates stopwatch ticks, not milliseconds. The consumer of the metric has to divide by <xref:System.Diagnostics.Stopwatch.Frequency> to obtain seconds.

Counting exceptions requires a `catch` block that rethrows.

[!metalama-file MeasureExceptionCountAttribute.cs]

## Step 2. Generating the metrics class

The type-level aspect does the rest of the work.

[!metalama-file ImplementMetricsAspect.cs member="ImplementMetricsAspect.BuildAspect"]

It performs five operations.

It introduces the companion class in the `Metrics` namespace, named after the target type. `whenExists: OverrideStrategy.New` keeps the aspect usable when a class of that name already exists.

It pulls two dependencies into the companion class, <xref:System.Diagnostics.Metrics.IMeterFactory> and `IMetricHost`, and introduces a dependency on the companion class itself into the instrumented type. The last one is declared with `IsRequired = false`, so the instrumented type keeps working when no metrics are configured. Dependency injection is provided by the `Metalama.Extensions.DependencyInjection` package, described in <xref:dependency-injection>.

It reads `builder.AspectInstance.Predecessors` to find the method-level aspects that required it. Each predecessor gives the aspect instance and the method it was applied to, which is the information a type-level aspect cannot obtain any other way.

For each predecessor, it introduces a field in the companion class, then overrides the method with the template of that predecessor. `WithTemplateProvider( predecessor.Aspect )` is the call that makes the method aspect's own template run under the type aspect.

It adds an annotation to the compilation that carries a reference to the generated class. The annotation is the channel to the assembly-level aspect of the next step, and the mechanism is described in <xref:sharing-state-with-advice>.

## Step 3. Creating the instruments

The constructor of the companion class is generated by the following template.

[!metalama-file ImplementMetricsAspect.cs member="ImplementMetricsAspect.InitializeMetrics"]

This template is the most instructive part of the example, because it calls another aspect's template. The `foreach` loop is compile-time, and inside it `metric.Aspect.CreateMetricTemplate( ... )` expands the template of the metric aspect at that point of the generated constructor. The technique is described in <xref:auxiliary-templates>.

The result is that the type-level aspect decides where the instrument is created, while each metric aspect decides how it is created. A new kind of measurement can be added by deriving from `MetricAttribute` alone, without changing `ImplementMetricsAspect`.

The host of the metrics is abstracted by the following interface. It supplies the identity of the application to the meter factory and receives each instrument as it is created, which lets an application keep its own inventory of instruments.

[!metalama-file IMetricHost.cs]

## Step 4. Registering the generated classes

Each generated metrics class is resolved through dependency injection, so each one needs to be registered. The assembly-level aspect generates that registration code.

[!metalama-file GenerateAddMetricsExtensionAttribute.cs]

It is applied once for the whole project.

[!metalama-file GlobalAspects.cs]

The template reads back the annotations added in step 2, so the generated method lists exactly the classes that were generated in the current project. `[This]` turns the first parameter into the receiver of an extension method, so the generated code is called as `serviceCollection.AddAutoGeneratedMetrics()`.

For the example above, the generated code is the following.

[!metalama-file Metrics.MetricRegistrations.cs transformed="true"]

## Limitations

The order in which the measurements are nested around the method body is the alphabetical order of `MetricKind`, not the aspect order declared with <xref:Metalama.Framework.Aspects.AspectOrderAttribute>. The predecessors are sorted by target method and then by metric kind, because the execution order of the predecessors is not exposed by `builder.AspectInstance.Predecessors`. The nesting is therefore deterministic but arbitrary, and a measurement that must be the outermost one cannot be placed there. Aspect ordering is described in <xref:ordering-aspects>.

The generated registration method takes <xref:Microsoft.Extensions.DependencyInjection.ServiceCollection>, the concrete class, rather than `IServiceCollection`. It cannot be called on a service collection exposed through the interface.

The three aspects all create a `Counter<long>`. Execution time would be better served by a histogram, which records the distribution of the measurements rather than their sum.

This example provides the `IMetricHost` contract but no implementation of it, and no runnable program. The implementation is where an application decides which meter identity to report and what to do with the instruments.

> [!div class="see-also"]
> - <xref:child-aspects>
> - <xref:auxiliary-templates>
> - <xref:dependency-injection>
> - <xref:sharing-state-with-advice>
> - <xref:ordering-aspects>
> - <xref:Metalama.Framework.Aspects.ITemplateProvider>
> - <xref:Metalama.Framework.Aspects.AspectQueryExtensions.RequireAspect*>
