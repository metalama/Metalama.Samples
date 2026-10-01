---
uid: sample-memento-2
level: 300
summary: "This article explains how to extend a Memento design pattern to support type inheritance using the Metalama framework, including validation and method overrides."
keywords: "Memento design pattern, IMementoable, nested type"
created-date: 2024-09-09
modified-date: 2024-09-09
---

# Memento example, step 2: supporting type inheritance

[!metalama-project-buttons .]

In this second article, we will see how to modify our aspect to support type inheritance.

## Strategizing

As always, we need to start reasoning and make some decisions about the implementation strategy before jumping into code.

We take the following approach:

* Each originator class will still have its own memento class, and these memento classes will inherit from each other. So if `Fish` derives from `FishtankArtifact`, then `Fish.Memento` will derive from `FishtankArtifact.Memento`. Therefore, memento classes will be `protected` and not `private`. Each memento class will be responsible for its own properties, not for the properties of the base class.
* Each `RestoreMemento` will be responsible only for the fields and properties of the current class and will call the `base` implementation to cope with properties of the base class.

## Result

When we are done with the aspect, it will transform code as follows.

Here is a base class:

[!metalama-compare FishtankArtifact.cs]

Here is a derived class:

[!metalama-compare Fish.cs]

## Step 1. Mark the aspect as inheritable

We certainly want our `[Memento]` aspect to automatically apply to derived classes when we add it to a base class. We achieve this by adding the <xref:Metalama.Framework.Aspects.InheritableAttribute?text=[Inheritable]> attribute to the aspect class.

[!metalama-file MementoAttribute.cs marker="ClassDefinition"]

For details about aspect inheritance, see <xref:aspect-inheritance>.

## Step 2. Validating the base type

If the base type already implements `IMementoable`, we need to check that the implementation fulfills our expectations. Indeed, it is possible that `IMementoable` is implemented _manually_. The following rules must be respected:

* There must be a `Memento` nested type in the base type.
* This nested type must be protected.
* This nested type must have a public or protected constructor accepting the base type as its only argument.

When doing any sort of validation, the first step is to define the errors we will use.

[!metalama-file DiagnosticDefinitions.cs]

We can then validate the code.

[!metalama-file MementoAttribute.cs marker="GetBaseType"]

For details regarding error reporting, see <xref:diagnostics>.

## Step 2. Specifying the OverrideAction

By default, advising methods such as <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceClass*> or <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod*> will fail if the same member already exists in the current or base type. To specify how the advising method should behave in this case, we must supply an <xref:Metalama.Framework.Aspects.OverrideStrategy> to the `whenExists` parameter. The default value is `Fail`. We must change it to `Ignore`, `Override`, or `New`:
* When using <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceClass*> to introduce the `Memento` nested class, we use `New`.
* When using <xref:Metalama.Framework.Aspects.AdviserExtensions.IntroduceMethod*> to introduce `SaveToMemento` or `RestoreMemento`, we use `Override`.
* When using <xref:Metalama.Framework.Aspects.AdviserExtensions.ImplementInterface*> to implement `IMemento` or `IMementoable`, we use `Ignore`.

## Step 3. Setting the base type and constructor of the Memento type

Now that we know if there is a valid base type, we can modify the logic that introduces the nested class and set the <xref:Metalama.Framework.Code.DeclarationBuilders.INamedTypeBuilder.BaseType> property.

[!metalama-file MementoAttribute.cs marker="IntroduceType"]

If we have a base class, we must also instruct the introduced constructor to call the base constructor. This is done by setting the <xref:Metalama.Framework.Code.DeclarationBuilders.IConstructorBuilder.InitializerKind> property. We then call the <xref:Metalama.Framework.Code.DeclarationBuilders.IConstructorBuilder.AddInitializerArgument*> method and pass the <xref:Metalama.Framework.Code.DeclarationBuilders.IParameterBuilder> returned by <xref:Metalama.Framework.Code.DeclarationBuilders.IMethodBaseBuilder.AddParameter*>.

[!metalama-file MementoAttribute.cs marker="IntroduceConstructor"]

## Step 4. Calling the base implementation from RestoreMemento

Finally, we must edit the `RestoreMemento` template to ensure it calls the `base` method if it exists. This can be done by simply calling `meta.Proceed()`. If a base method exists, it will call it. Otherwise, this call will be ignored.

[!metalama-file MementoAttribute.cs member="MementoAttribute.RestoreMemento"]

## Complete aspect

Here is the `MementoAttribute`, now supporting class inheritance.

[!metalama-file MementoAttribute.cs]



