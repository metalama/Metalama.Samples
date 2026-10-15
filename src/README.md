---
uid: samples
summary: "The document provides tutorial examples for using Metalama, covering validation, logging, caching, exception handling, property change notification, change tracking, cloning, the memento, builder and singleton patterns, equality comparison, enum view-models, shared fabrics, ToString implementation, call tracing, interception proxies, metrics instrumentation, and MAUI exception tracking."
keywords: "Metalama examples, Metalama samples"
created-date: 2023-03-28
modified-date: 2026-10-15
---

# Commented examples

This chapter shows a few examples that demonstrate how to use Metalama. Each of them is designed as a tutorial, progressively introducing features of growing complexity. You can use these examples to support your learning process. 

| Example                                                   | Description                                                                                                     |
|-----------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| [Validation](validation/README.md)                        | Simple aspects relative to validation or normalization of fields, properties, or parameters.
| [Logging](log/README.md)                                  | Shows several logging aspects, adding complexity at every step.                                                |
| [Caching](caching/README.md)                              | Caches the method return value as a function of its parameters.                                                 |
| [Exception Handling](exception-handling/README.md)        | Demonstrates several exception-handling strategies including retry, [Polly](https://github.com/App-vNext/Polly), and adding parameter values for richer reports. |
| [INotifyPropertyChanged](notifypropertychanged/README.md) | Automatically implements the <xref:System.ComponentModel.INotifyPropertyChanged> interface and instruments properties. |
| [Change Tracking](change-tracking/README.md)              | Automatically implements the <xref:System.ComponentModel.IChangeTracking> or <xref:System.ComponentModel.IRevertibleChangeTracking> interface  interface and instruments fields and properties accordingly.                      |
| [Clone](clone/README.md)                                  | Implements the _Deep Clone_ pattern.                                                                             |
| [Memento](memento/README.md)                              | Implements the classic _Memento_ pattern.
| [Enum View-Model](enum-viewmodel/README.md)               | Creates a view-model class to wrap an enum value.                                                                |
| [Shared Fabric](fabrics/shared/README.md)                 | Demonstrates a fabric that targets several projects. Contributed by Whit Waldo. |
| [ToString](tostring/README.md)                            | Implements the <xref:System.Object.ToString*> method.                                                           |
| [Builder](builder/README.md)                              | Implements the Builder pattern, i.e. a mutable `Builder` type nested inside an immutable type. |
| [Equatable](comparison/README.md)                            | Implements the complete equality comparison pattern, including the `IEquatable<T>` method, and more. |
| [Tracing calls](call-tracing/README.md)                   | Traces the calls to a library from one file, with interceptors, to investigate a problem without a debugger. |
| [Interception proxies](Metalama.Samples.Proxy/README.md)  | Generates an interception proxy for an interface at compile time, without run-time code generation. |
| [Metrics](metrics/Metalama.Samples.Metrics/README.md)     | Instruments methods with `System.Diagnostics.Metrics` counters and generates their dependency injection registration. |
| [MAUI Exception Tracking](maui-exception-tracking/README.md) | Tracks exceptions in a .NET MAUI application and records the visual element that caused each one. |

If you have developed an original aspect with Metalama, consider [contributing](contributing.md) to this list.



