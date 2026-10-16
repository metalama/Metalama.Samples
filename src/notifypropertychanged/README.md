---
uid: sample-notifypropertychanged
summary: "This section shows two ways to implement INotifyPropertyChanged with Metalama: the ready-made Observable aspect for production code, and a custom aspect built step by step for learning purposes."
keywords: "INotifyPropertyChanged, Metalama.Patterns.Observability, Observable, OnPropertyChanged, MVVM, data binding, aspect"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Examples: INotifyPropertyChanged

<xref:System.ComponentModel.INotifyPropertyChanged> is the standard .NET interface for the Observable pattern, and most
.NET UI frameworks rely on it, including WPF, WinForms, WinUI and Blazor. Implementing it by hand requires a large
amount of boilerplate code, because each property must raise
the <xref:System.ComponentModel.INotifyPropertyChanged.PropertyChanged> event for itself and for every property that
depends on it. This section shows two ways to generate that code with Metalama.

## Choosing between the ready-made aspect and a custom one

For production code, use the <xref:Metalama.Patterns.Observability.ObservableAttribute?text=[Observable]> aspect of the
open-source `Metalama.Patterns.Observability` package. It analyzes the dependencies between properties, supports
computed properties, child objects and constant methods, and reports a warning when it encounters a construct it does
not support. The first example below shows how to apply it. The reference documentation of this package
is <xref:observability>.

The other examples build a `NotifyPropertyChanged` aspect from scratch. Their objective is didactic: they teach the
aspect framework, starting from the simplest possible implementation and then addressing its limitations. Read them to
understand how an aspect of this kind works. Do not copy them into an application, because the ready-made aspect
handles cases that these examples deliberately leave out.

| Example                                                                                                      | Description                                                                                                                       |
|--------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| [Using the ready-made aspect](observability/README.md)                                                       | Applies the `[Observable]` aspect of `Metalama.Patterns.Observability` to a type that has a computed property.                     |
| [Custom aspect, step 1: Basic implementation](custom/README.md)                                              | Builds a `NotifyPropertyChanged` aspect that implements the interface and overrides all property setters. Ignores dependencies.    |
| [Custom aspect, step 2: Dependencies within the type](../sdk/notifypropertychanged-sdk-1/README.md)          | Uses `Metalama.Framework.Sdk` to detect the dependencies between the properties of the type.                                       |
| [Custom aspect, step 3: Dependencies on base class properties](../sdk/notifypropertychanged-sdk-2/README.md) | Extends the analysis to the properties of the base class, and moves the dependency logic into `OnPropertyChanged`.                 |

> [!div class="see-also"]
> <xref:observability>
