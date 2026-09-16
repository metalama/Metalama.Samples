---
uid: sample-tostring-1
summary: "This document explains how to implement a `[ToString]` aspect in Metalama to generate a `ToString` method based on public fields and properties."
keywords: "ToString method, generate code"
created-date: 2024-09-05
modified-date: 2024-09-09
---

# ToString example, step 1: Getting started

[!metalama-project-buttons .]

This example gives the simplest possible implementation of the `[ToString]` aspect, which generates the `ToString` method based on public fields and properties of the current type.

Our objective is to be able to generate code like this:

[!metalama-compare MovingVertex.cs ]

## Aspect implementation

The aspect is quite straightforward. Here its complete source code:

[!metalama-file ToStringAttribute.cs ]

Because we want to apply this aspect to types, we derive the `ToStringAttribute` class from <xref:Metalama.Framework.Aspects.TypeAspect>.

The only member of this type is the `IntroducedToString` method: the template for the new `ToString` method. 

We use <xref:Metalama.Framework.Aspects.IntroduceAttribute?text=[Introduce]> on the top of this method to mean that the method must be introduced to the target class.

Note that we cannot name this template method `ToString` because the `ToString` method already exists in the `object` class and is obviously not a template. Therefore, we must set the `Name` property of <xref:Metalama.Framework.Aspects.IntroduceAttribute?text=[Introduce]> to say that the name of the introduced method differs from the name of the template method.

We use `WhenExists = OverrideStrategy.Override` to ask Metalama to _override_ the method if it already exists in the base type, which is obviously always the case.

The template implementation relies on <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder> to generate an interpolated string. Note that <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder> is a compile-time class. 

The following line may look weird at first sight:

[!metalama-file ToStringAttribute.cs marker="CompileTimeVariable" ]

It defines a compile-time variable. Without the call to <xref:Metalama.Framework.Aspects.meta.CompileTime*?text=meta.CompileTime>, a run-time local variable would be defined because, when an expression can be both run-time or compile-time (as can be `0`), it is considered run-time by default.

We enumerate the <xref:Metalama.Framework.Code.INamedType.AllFieldsAndProperties> collection, which contains all fields and properties of the current type, including all those inherited from the base type (the <xref:Metalama.Framework.Code.INamedType.FieldsAndProperties>) only contains members of the current type, ignoring those of the base type).

For each public field or property, we call <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder.AddText*> to add the member name, then <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder.AddExpression*> to add the member value.

At the end of the method, we call the <xref:Metalama.Framework.Code.SyntaxBuilders.ExpressionBuilderExtensions.ToValue*> method to build a run-time interpolated string from the compile-time <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder>, which is the return value of our `ToString` method.



