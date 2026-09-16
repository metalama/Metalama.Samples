---
uid: sample-clone-1
summary: "This document outlines the initial steps to create a `Cloneable` aspect in C#, focusing on deep cloning by marking child properties with a custom `[Child]` attribute."
keywords: "Cloneable aspect, deep cloning, ICloneable interface, MemberwiseClone"
created-date: 2023-04-21
modified-date: 2024-09-09
---

# Clone example, step 1: getting started

[!metalama-project-buttons .]

This article will create the first working version of the `Cloneable` aspect. Once it is done, it will implement the
Deep Clone pattern as shown below:

[!metalama-files Game.cs GameSettings.cs]

Before we start writing the aspect, we must materialize in C# the concept of a _child property_. Conceptually, a child
property is a property that points to a reference-type object that needs to be cloned when the parent object is cloned.
Let's decide to mark such properties with the `[Child]` custom attribute:

[!metalama-file ChildAttribute.cs]

## Aspect implementation

The whole aspect implementation is here:

[!metalama-file CloneableAttribute.cs]

The `BuildAspect` method is the entry point of the aspect.

You can clearly see two steps in this method. We will comment on them independently.

### Implementing the interface

The first operation of `BuildAspect` is to add the <xref:System.ICloneable> method to the current type using
the <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> method.

[!metalama-file CloneableAttribute.cs marker="BuildAspect1"]

If the type already implements the <xref:System.ICloneable> method, we don't need to do anything, so we are
specifying `Ignore` as the <xref:Metalama.Framework.Aspects.OverrideStrategy>.
The <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> method requires the aspect type to include
all
interface members and to annotate them with
the <xref:Metalama.Framework.Aspects.InterfaceMemberAttribute?text=[InterfaceMember]> custom attribute.

Our interface implementation calls the public `Clone` method we will introduce in the type.

[!metalama-file CloneableAttribute.cs member="CloneableAttribute.Clone"]

For details, see <xref:implementing-interfaces>.

Note that the code uses the expression <xref:Metalama.Framework.Aspects.meta.This?text=meta.This>, a _compile-time_
expression that returns a `dynamic` value. Thanks to its `dynamic` nature, you can write any run-time expression on its
right side. This code is not verified until all aspects have been executed, so you can call a method that does not exist
yet. For details regarding these techniques, see <xref:dynamic-typing>

### Adding the public method

The second operation of `BuildAspect` is to introduce a method named `Clone` by
invoking <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod*>.

[!metalama-file CloneableAttribute.cs marker="BuildAspect2"]

We set the <xref:Metalama.Framework.Aspects.OverrideStrategy> to `Override`, indicating that the method should be
overridden if it already exists in the type. The invocation
of <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod*> is more complex than usual for two reasons:

1. The template method cannot be named `Clone` because it would conflict with the other `Clone` method of this aspect,
   the template for the <xref:System.ICloneable.Clone*?text=ICloneable.Clone> method. Therefore, we name the template
   method `CloneImpl` and rename the introduced method using the delegate passed to the `buildMethod` parameter. Hence,
   the code `buildMethod: m => m.Name = "Clone"`.

2. The `CloneImpl` template, as we will see below, has a compile-time generic parameter `T`, where `T` represents the
   current type. We need to pass the value of the `T` parameter in our invocation to
   the <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod*> method. We pass an anonymous type to
   the `args` parameter, with the property `T` set to its desired value:  `args: new { T = builder.Target }`.

For details, see <xref:introducing-members>.

Let's now examine the `CloneImpl` template:

[!metalama-file CloneableAttribute.cs member="CloneableAttribute.CloneImpl"]

The first half of the method generates the _base_ call with two possibilities:

* When the method is an _override_: `var clone = (T) base.Clone();`
* Otherwise, when the base type is not deeply clonable: `var clone = (T) this.MemberwiseClone();`

<xref:System.Object.MemberwiseClone*> is a standard method of the <xref:System.Object> class. It returns a shallow copy
of the current object. Using the <xref:System.Object.MemberwiseClone*> has many benefits:

* It is faster than setting individual fields or properties in C#.
* It works even when the base type is unaware of the Clone pattern.

<xref:Metalama.Framework.Aspects.meta.This?text=meta.Base> works similarly to the xref:
Metalama.Framework.Aspects.meta.This?text=meta.This> we already used before. It returns a `dynamic` value, and anything
you write on its right side becomes a run-time expression, i.e., C# code injected by the template. To convert this code
into a compile-time <xref:Metalama.Framework.Code.IExpression> object, we cast the `dynamic` expression
into <xref:Metalama.Framework.Code.IExpression>.

The second part of the `CloneImpl` template selects all fields and properties annotated with the `[Child]` attribute and
generates code according to the pattern `clone.Foo = (FooType?) this.Foo?.Clone()`. Fields or properties are represented
as compile-time objects by the <xref:Metalama.Framework.Code.IFieldOrProperty> interface.
The <xref:Metalama.Framework.Code.IExpression.Value> property operates the same kind of magic as `meta.This`
or `meta.Base` above, i.e., a `dynamic` property that can be used in run-time code. By default, `field.Value` generates
a reference to the field for the current instance (i.e. `this.Foo`). To get the field for a different instance (
e.g. `clone.Foo`), you must use <xref:Metalama.Framework.Code.Invokers.IFieldOrPropertyInvoker.With*>.

## Summary

In this article, we have created a `Cloneable` aspect that performs deep cloning of an object by recursively calling
the `Clone` method on child properties. However, we did not validate that the child objects actually have a _Clone_
method or that child properties are not read-only. We will address this problem the following step.

> [!div class="see-also"]
> <xref:introducing-members>
> <xref:implementing-interfaces>
> <xref:dynamic-typing>



