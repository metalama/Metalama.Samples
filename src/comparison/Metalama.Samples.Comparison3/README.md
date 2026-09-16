---
uid: sample-comparison-3
---

# Equality comparison example, step 3: Hand-picking equality members

In the [first article](xref:sample-comparison-1) of this series, we created an aspect that automatically implements equality comparison for types, including all type fields and automatic properties in the comparison.

In this article, we'll show you how to modify the aspect so you can hand-pick the members that should be part of <xref:System.Object.Equals%2A> and <xref:System.Object.GetHashCode>, using a new custom attribute `[EqualityMember]`.

## Step 1: Adding EqualityMemberAttribute

If we wanted to keep it simple, `EqualityMemberAttribute` could be a plain C# custom attribute class. However, we're adding a bit of complexity to our aspect library to improve the user experience:

- We want the `[EqualityMember]` aspect to automatically add the `ImplementEquatable` aspect to the type.
- We want to ensure that `[EqualityMember]` is only added to non-static fields or properties and report an error if otherwise.

To make this happen, we derive the `EqualityMemberAttribute` class from <xref:Metalama.Framework.Aspects.TypeAspect>.

In <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A>, we call the <xref:Metalama.Framework.Aspects.AdviserExtensions.RequireAspect%2A> method to implicitly add the `ImplementEquatable` aspect to the type, if it isn't already added.

To define valid targets for this attribute, we implement the <xref:Metalama.Framework.Aspects.TypeAspect.BuildEligibility%2A> method.

[!metalama-file EqualityMemberAttribute.cs]

You can learn more about these techniques in <xref:child-aspects> and <xref:eligibility>.

For the `RequireAspect` method to work, we must ensure that Metalama processes `EqualityMemberAttribute` aspects _before_ `ImplementEquatableAttribute`; otherwise, it would be too late for `EqualityMemberAttribute` to add an `ImplementEquatableAttribute` aspect. This is done by using the `[assembly: AspectOrder]` custom attribute:

[!metalama-file AspectOrder.cs]

To read more about aspect ordering, see <xref:ordering-aspects>.

## Step 2: Modifying the BuildAspect method

Now, we can update the logic that selects equality members in the <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A> method. Naturally, we'll only select those that have the `[EqualityMember]` attribute.

[!metalama-file ImplementEquatableAttribute.cs marker="FindFields"]

What should happen when this query returns an empty set, i.e., the user did not tag any field or property with `[EqualityMember]`? The answer depends on the situation:

- If the user _explicitly_ used the `[ImplementEquatable]` attribute on the type but omitted to mark any field, this is certainly an error that should be reported.
- If the `[ImplementEquatable]` aspect was _inherited_ from a base type, and the current field does not add any equality member, then there's nothing to do: no error to report, nor any code transformation to perform. We can just ignore the aspect.

This is implemented by the following code in <xref:Metalama.Framework.Aspects.TypeAspect.BuildAspect%2A>:

[!metalama-file ImplementEquatableAttribute.cs marker="ValidateFields"]

Here is the error definition:

[!metalama-file DiagnosticDefinitions.cs member="DiagnosticDefinitions.NoEqualityMemberError"]

And that's all! There's nothing else to change. This is the beauty of separating analysis from advising: you can change the collection of equality members, and the rest of the aspect will simply consume it. As you can see, standard best practices also apply to meta-programming, especially separation of concerns.

## Summary

We've defined a new custom attribute `[EqualityMember]`. We made it a <xref:Metalama.Framework.Aspects.TypeAspect> more for convenience than necessity, to utilize validation and implicitly add the "parent" aspect.

Then, we simply changed the logic that built the collection of equality members so that it checks for the presence of a custom attribute of type `EqualityMemberAttribute`.

In the [next article](xref:sample-comparison-4), we'll extend the `EqualityMemberAttribute` aspect to make it possible to customize the equality contract.

> [!div class="see-also"]
> <xref:child-aspects>
> <xref:eligibility>
> <xref:ordering-aspects>
