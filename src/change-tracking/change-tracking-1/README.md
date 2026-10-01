---
uid: sample-dirty-1
summary: "This article discusses creating an aspect to implement the `IChangeTracking` interface, focusing on design decisions and implementation details."
keywords: "change tracking, .NET, IChangeTracking interface, aspect"
created-date: 2023-04-18
modified-date: 2024-09-09
---

# Change Tracking Example, Step 1: Getting Started

[!metalama-project-buttons .]

This article will create an aspect that automatically implements the <xref:System.ComponentModel.IChangeTracking>
interface of the .NET Framework.

Before implementing any aspect, we must first discuss the implementation design of the pattern. The main design decision
is that objects will not track their changes by default. Instead, they will have an `IsTrackingChanges` property to
control this behavior. Why? Because if we enable change tracking by default, we will need to reset the changes after
object initialization has been completed, and we have no way to automate this. So, instead of asking users to write code
that calls the <xref:System.ComponentModel.IChangeTracking.AcceptChanges*> method after each object initialization, we
ask them to set the `IsTrackingChanges` property to `true` if they are interested in change tracking. Therefore, we can
define the `ISwitchableChangeTracking` interface as follows:

[!metalama-file ISwitchableChangeTracking.cs]

We want our aspect to generate the following code. In this example, we have a base class named `Comment` and a derived
class named `ModeratedComment`.

[!metalama-files Comment.cs ModeratedComment.cs]

## Aspect Implementation

Our aspect implementation needs to perform two operations:

1. Implement the `ISwitchableChangeTracking` interface (including the <xref:System.ComponentModel.IChangeTracking>
   system interface) unless the type already implements it;
2. Add an `OnChange` method that sets the <xref:System.ComponentModel.IChangeTracking.IsChanged> property to `true` if
   change tracking is enabled unless the type already contains such a method;
3. Override the setter of all fields and automatic properties to call `OnChange`.

Here is the complete implementation:

[!metalama-file TrackChangesAttribute.cs]

The `TrackChangesAttribute` class is a type-level aspect, so it must derive from
the <xref:Metalama.Framework.Aspects.TypeAspect> class, which itself derives from <xref:System.Attribute>.

The <xref:Metalama.Framework.Aspects.InheritableAttribute?text=[Inheritable]> at the top of the class indicates that the
aspect should be inherited from the base class to derived classes. For further details, see <xref:aspect-inheritance>.

The entry point of the aspect is the `BuildAspect` method. Our implementation has two parts, two of the three operations
that our aspect has to perform.

First, the `BuildAspect` method calls the <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*>
method
to add the `ISwitchableChangeTracking` interface to the target type. It specifies
the <xref:Metalama.Framework.Aspects.OverrideStrategy> to `Ignore`, indicating that the operation should be ignored if
the target type already implements the interface.
The <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> method requires the aspect class to contain
the interface members, which should be annotated with
the <xref:Metalama.Framework.Aspects.InterfaceMemberAttribute?text=[InterfaceMember]> custom attribute. The
implementation of these members is trivial. For details about adding interfaces to types,
see <xref:implementing-interfaces>.

Then, the `BuildAspect` method selects fields and automatic properties except `readonly` fields and `init` or `get`-only
automatic properties (we apply this condition using the expression `f.Writeability == Writeability.All`). For all these
fields and properties, we call the <xref:Metalama.Framework.Aspects.AdviserExtensions.OverrideAccessors*> method
using `OverridePropertySetter` as a template for the new setter. For further details,
see <xref:overriding-fields-or-properties>.

Here is the template for the field/property setter. Note that it must be annotated with
the <xref:Metalama.Framework.Aspects.TemplateAttribute?text=[Template]> custom attribute.

[!metalama-file TrackChangesAttribute.cs member="TrackChangesAttribute.OverrideSetter"]

To introduce the `OnChange` method (which is not part of the interface), we use the following code:

[!metalama-file TrackChangesAttribute.cs member="TrackChangesAttribute.OnChange"]

The `OnChange` method has an <xref:Metalama.Framework.Aspects.IntroduceAttribute?text=[Introduce]> custom attribute;
therefore, Metalama will add this method to the target type. We again assign the `Ignore` value to
the <xref:Metalama.Framework.Aspects.IntroduceAttribute.WhenExists> property to skip this step if the target type
already contains this method.

## Summary

In this first article, we created the first version of the `TrackChanged` aspect, which already does a good job. The
aspect also works with manual implementations of <xref:System.ComponentModel.IChangeTracking> as long as the
OnChange` method is available. But what if there is no `OnChange` method? In the following article, we will see how to
report an error when this happens.

> [!div class="see-also"]
> <xref:aspect-inheritance>
> <xref:implementing-interfaces>
> <xref:overriding-fields-or-properties>

  



