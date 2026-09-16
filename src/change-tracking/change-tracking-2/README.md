---
uid: sample-dirty-2
summary: "This document explains enhancing an aspect to report errors if a base class lacks a required `OnChange` method or if it's not protected."
keywords: ".NET, aspect, IChangeTracking interface"
created-date: 2023-04-18
modified-date: 2024-09-09
---

# Change Tracking example, step 2: verifying manual code

[!metalama-project-buttons .]

In the previous article, we created an aspect that automatically implements
the <xref:System.ComponentModel.IChangeTracking>  interface. If the base class has a manual implementation of
the <xref:System.ComponentModel.IChangeTracking>, the aspect will still work correctly and call the `OnChange` method of
the base class. However, what if the base class does _not_ contain an `OnChange` method or if it is not protected? Let's
improve the aspect and report an error in these situations.

The result of this aspect will be two new errors:

[!metalama-test ../../tests/Metalama.Samples.ChangeTracking2.Tests/MissingOnChangeMethod.cs]

[!metalama-test ../../tests/Metalama.Samples.ChangeTracking2.Tests/OnChangeMethodNotProtected.cs]

## Aspect implementation

[!metalama-file TrackChangesAttribute.cs]

The first thing we add to the `TrackChangesAttribute` is two static fields to _define_ the errors:

[!metalama-file TrackChangesAttribute.cs member="TrackChangesAttribute._mustHaveOnChangeMethod"]
[!metalama-file TrackChangesAttribute.cs member="TrackChangesAttribute._onChangeMethodMustBeProtected"]

Metalama requires the <xref:Metalama.Framework.Diagnostics.DiagnosticDefinition> to be defined in a static field or
property. To learn more about reporting errors, see <xref:diagnostics>.

Then, we add this code to the `BuildAspect` method:

[!metalama-file TrackChangesAttribute.cs marker="BuildAspect"]

As in the previous step, the `BuildAspect` method
calls <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> with
the `Ignore` <xref:Metalama.Framework.Aspects.OverrideStrategy>. This time, we inspect the outcome
of <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*>. If the outcome is `Ignored`, it means that
the
type or any base type already implements the <xref:System.ComponentModel.IChangeTracking>  interface. In this case, we
check that the type contains a parameterless method named `OnChange` and verify its accessibility.

## Summary

This article explained how to report an error when the source code does not meet the expectations of the aspect. To make
our aspect usable in practice, i.e., to make it possible to enable or disable a hypothetical _Save_ button when the user
performs changes in the UI, we still have to integrate with the <xref:System.ComponentModel.INotifyPropertyChanged>
interface and raise the <xref:System.ComponentModel.INotifyPropertyChanged.PropertyChanged> event when
the <xref:System.ComponentModel.IChangeTracking.IsChanged> property changes. We will see how to do this in the following
article.

> [!div class="see-also"]
> <xref:diagnostics>



