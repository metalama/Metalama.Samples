---
uid: sample-notifypropertychanged-sdk-2
summary: "This document describes a second implementation strategy of `INotifyPropertyChanged` using the Roslyn API to analyze the syntax tree, supporting base class property dependencies."
keywords: "INotifyPropertyChanged, Roslyn API, syntax tree, OnPropertyChanged"
created-date: 2024-05-13
modified-date: 2024-09-09
---

# Example: INotifyPropertyChanged with dependencies (2)

This is a second implementation strategy of `INotifyPropertyChanged` that uses the Roslyn API to analyze the syntax
tree.
This design supports dependencies on properties of the base class, and moved the smart logic into `OnPropertyChanged`.



