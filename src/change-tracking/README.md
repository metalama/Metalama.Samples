---
uid: sample-dirty
summary: "The document explains the _Dirty Flag_ pattern for tracking object state changes, its implementation in .NET, and automating it using aspect-oriented programming."
keywords: "Dirty Flag pattern, c# change tracking, IChangeTracking, IReversibleChangeTracking"
created-date: 2023-04-18
modified-date: 2024-09-09
---

# Implementing the Dirty Flag pattern (IChangeTracking) without boilerplate

The _Dirty Flag_ pattern is a design pattern that tracks changes to an object's state by maintaining a boolean flag indicating whenever the object's properties or fields have been modified. This pattern is commonly used in user interface programming, where it is necessary to quickly determine whether an object's state has changed so that the `Save` button can be dynamically enabled. In .NET, objects that support this feature must implement the <xref:System.ComponentModel.IChangeTracking> interface.

Implementing the _Dirty Flag_ pattern typically requires adding a large amount of boilerplate code to each class that needs to track changes to its state. However, aspect-oriented programming (AOP) techniques can automate this repetitive and error-prone task. In this example, we will see how to create an aspect that automates this pattern, reducing the boilerplate code you must write and maintain while improving code quality and consistency.

| Article | Description |
| --- | --- |
| [1. Getting started](change-tracking-1/README.md) | Gives a minimal implementation of the change tracking pattern. |
| [2. Verifying code](change-tracking-2/README.md) | Reports errors if the manual implementation of the pattern does not respect the conventions of the pattern. |
| [3. Integrating with INotifyPropertyChanged](change-tracking-3/README.md) | Raises `PropertyChange` notifications when the `IsChanged` property has changed. |
| [4. Reverting changes](change-tracking-4/README.md) | Adds support for the `RevertChanges` method. |
| [Limitations](limitations.md) | This article discusses the limitations of the above aspects. |



