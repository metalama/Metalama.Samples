---
uid: sample-enum-viewmodel
level: 200
summary: "This document describes creating an aspect in Metalama to generate view-model classes for `enum` types, producing `IsFoo` properties for each enum member."
keywords: "enum view-model, aspect, generate view-model, enum member"
created-date: 2023-03-28
modified-date: 2024-09-09
---

# Generating view-model wrappers for enums without boilerplate

[!metalama-project-buttons .]

This example shows how to build an aspect that generates view-model classes for `enum` types. For each enum member `Foo`, the aspect will generate an `IsFoo` property.

For instance, let's take the following input `enum`:

[!metalama-file Visibility.cs]

The aspect will generate the following output:

[!metalama-file ViewModels.VisibilityViewModel.cs transformed="true"]

## Step 1. Creating the aspect class and its properties

We want to use the aspect as an assembly-level custom attribute, as follows:

[!metalama-file GlobalAspects.cs]

To create an assembly-level aspect, we need to derive the <xref:Metalama.Framework.Aspects.CompilationAspect> class.

We add two properties to the aspect class: `EnumType` and `TargetNamespace`. We initialize them from the constructor:

[!metalama-file GenerateEnumViewModelAttribute.cs marker="ClassHeader"]

## Step 2. Coping with several instances

By default, Metalama supports only a single instance of each aspect class per target declaration. To allow for several instances, we must first add the <xref:System.AttributeUsageAttribute?text=[AttributeUsage]> custom attribute to our aspect.

[!metalama-file GenerateEnumViewModelAttribute.cs marker="AttributeUsage"]

Regardless of the number of `[assembly: GenerateEnumViewModel]` attributes found in the project, Metalama will call `BuildAspect`, the aspect entry point method, for only one of these instances. The other instances are known as _secondary instances_ and are available under `builder.AspectInstance.SecondaryInstances`.

Therefore, we will add most of the implementation in a local function named `ImplementViewModel` and invoke this method for both the primary and the secondary methods. Our `BuildAspect` method starts like this:

[!metalama-file GenerateEnumViewModelAttribute.cs marker="MultiInstance"]

The next steps will show how to build the `ImplementViewModel` local function.

## Step 3. Validating inputs

It's good practice to verify all assumptions and report clear error messages when the aspect user provides unexpected inputs. Here, we verify that the given type is indeed an `enum` type.

First, we declare the error messages as a static field of a compile-time class:

[!metalama-file DiagnosticDefinitions.cs]

Then, we check the code and report the error if something is incorrect:

[!metalama-file GenerateEnumViewModelAttribute.cs marker="ValidateInputs"]

For details on reporting errors, see <xref:diagnostics>.

## Step 4. Introducing the class, the value field, and the constructor

We can now introduce the view-model class using the <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceClass*> method. This returns an object that we can use to add members to the value field (using <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceField*>) and the constructor (using <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceConstructor*>).

[!metalama-file GenerateEnumViewModelAttribute.cs marker="IntroduceClass"]

For details, see <xref:introducing-types> and <xref:introducing-members>.

Here is the T# template of the constructor:

[!metalama-file GenerateEnumViewModelAttribute.cs marker="ConstructorTemplate"]

Note that this template accepts a compile-time generic parameter `T`, which represents the `enum` type. The value of this parameter is set in the call to <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceConstructor*> by setting the `args` parameter.

In this template, `meta.This._value` compiles to `this._value`. The C# compiler does not complain because `meta.This` returns a `dynamic` value, so we can have anything on the right hand of this expression. Metalama then just replaces `meta.This` with `this`.

## Step 5. Introducing the view-model properties

We can finally add the `IsFoo` properties. Depending on whether the enum is a multi-value bit map (i.e., `[Flags]`) or a single-value type, we need different strategies.

Here is the code that adds the properties:

[!metalama-file GenerateEnumViewModelAttribute.cs marker="AddProperties"]

The code first selects the proper template depending on the nature of the enum type.

Then, it enumerates the enum members, and for each member, calls the <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceProperty*> method. Note that we are passing a `member` tag, which will be used by the templates.

Here is the template for the non-flags variant:

[!metalama-file GenerateEnumViewModelAttribute.cs member="GenerateEnumViewModelAttribute.IsMemberTemplate"]

Here is the template for the flags variant:

[!metalama-file GenerateEnumViewModelAttribute.cs member="GenerateEnumViewModelAttribute.IsFlagTemplate"]

In this template, `meta.Tags["member"]` refers to the `member` tag passed by `BuildAspect`.

## Complete aspect

Putting all the pieces together, here is the complete code of the aspect:

[!metalama-file GenerateEnumViewModelAttribute.cs]



