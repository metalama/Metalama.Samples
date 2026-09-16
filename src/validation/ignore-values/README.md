---
uid: sample-ignore-value
keywords: "ignore values, forbidden value, .NET"
created-date: 2023-03-28
modified-date: 2024-09-09
---

# Automatically ignoring property values without boilerplate

[!metalama-project-buttons .]

This very sample aspect overrides the target field or property so that any attempt to set it to one of the forbidden value is simply ignored.

[!metalama-compare Author.cs ]

## Implementation

The aspect class is derived from <xref:Metalama.Framework.Aspects.FieldOrPropertyAspect>, which itself derives from <xref:System.Attribute>. 

The aspect constructor accepts the list of forbidden values, and stores them as a field.

[!metalama-file IgnoreValuesAttribute.cs marker="Constructor"]

The <xref:Metalama.Framework.Aspects.OverrideFieldOrPropertyAspect.OverrideProperty> property is the template overriding the original property. The getter implementation, `get => meta.Proceed()`, means that the getter is not modified. In the setter, we have a compile-time `foreach` loop that, for each forbidden value, tests if the assigned value is equal to this forbidden value and, if it is the case, returns before calling `meta.Proceed()`, i.e. before assigning the underlying field.

[!metalama-file IgnoreValuesAttribute.cs member="IgnoreValuesAttribute.OverrideProperty"]

This simple approach works well for most types you can use in an attribute constructor, but not for all of them:

- For enums (except .NET Standard 2.0 enums), the constructor will receive the _underlying integer_ value instead of a typed value. This means that our comparison will generate invalid C# because it will compare an enum to an integer.
- For arrays, a simple `==` comparison is not sufficient.

Both cases could be handled by a more complex aspect. However, in this example, we prevent the aspect from being applied to fields or properties of an unsupported type. We achieve this by implementing the <xref:Metalama.Framework.Eligibility.IEligible`1.BuildEligibility*> method.

[!metalama-file IgnoreValuesAttribute.cs member="IgnoreValuesAttribute.BuildEligibility"]

## Complete source code

Here is the complete source code of the aspect.

[!metalama-file IgnoreValuesAttribute.cs]

