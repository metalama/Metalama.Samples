---
uid: sample-log-5
level: 300
summary: "The document explains implementing an aspect that requires an existing `ILogger` field, reporting errors if missing or incorrect, and handling eligibility for static methods."
keywords: "logging aspect, eligibility"
created-date: 2023-04-05
modified-date: 2024-09-09
---

# Logging example, step 5: ILogger without dependency injection

[!metalama-project-buttons .]

Let's take a step back. In the previous example, we used the heavy magic of
the <xref:Metalama.Extensions.DependencyInjection.IntroduceDependencyAttribute?text=[IntroduceDependency]> custom
attribute. In this new aspect, the aspect will require an _existing_ `ILogger` field to exist and will report errors if
the target type does not meet expectations.

In the following code snippet, you can see that the aspect reports an error when the field or property is missing.

[!metalama-file ../../tests/Metalama.Samples.Log5.Tests/MissingFieldOrProperty.cs]

The aspect also reports an error when the field or property is not of the expected type.

[!metalama-file ../../tests/Metalama.Samples.Log5.Tests/FieldOfWrongType.cs]

Finally, the aspect must report an error when applied to a static method.

[!metalama-file ../../tests/Metalama.Samples.Log5.Tests/StaticMethod.cs]

## Implementation

How can we implement these new requirements? Let's look at the new implementation of the aspect.

[!metalama-file LogAttribute.cs]

We added some logic to handle the `ILogger` field or property.

### LogAttribute class

First, the `LogAttribute` class is now derived from <xref:Metalama.Framework.Aspects.MethodAspect> instead
of <xref:Metalama.Framework.Aspects.OverrideMethodAspect>. Our motivation for this change is that we need
the `OverrideMethod` method to have a parameter not available in
the <xref:Metalama.Framework.Aspects.OverrideMethodAspect> method.

### BuildAspect method

The `BuildAspect` is the entry point of the aspect. This method does the following things:

* First, it looks for a field named `_logger` or a property named `Logger`. Note that it uses
  the <xref:Metalama.Framework.Code.INamedType.AllFields> and <xref:Metalama.Framework.Code.INamedType.AllProperties>
  collections, which include the members inherited from the base types. Therefore, it will also find the field or
  property defined in base types.

* The `BuildAspect` method reports an error if no field or property is found. Note that the error is defined as a static
  field of the class. To read more about reporting errors, see <xref:diagnostics>.

* Then, the `BuildAspect` method verifies the type of the `_logger` field or property and reports an error if the type
  is incorrect.

* Finally, the `BuildAspect` method overrides the target method by
  calling <xref:Metalama.Framework.Aspects.AdviserExtensions.Override*?text=builder.Override> and by passing the
  name of the template method that will override the target method. It passes
  the <xref:Metalama.Framework.Code.IFieldOrProperty> by using an anonymous type as the `args` parameter, where the
  property names of the anonymous type must match the parameter names of the template method.

To learn more about imperative advising of methods, see <xref:overriding-methods>. To learn more about template
parameters, see <xref:template-parameters>.

### OverrideMethod method

The `OverrideMethod` looks familiar, except for a few differences. The field or property that references the `ILogger`
is given by the `loggerFieldOrProperty` parameter, which is set from the `BuildAspect` method. This parameter is
of <xref:Metalama.Framework.Code.IFieldOrProperty> type, which is a compile-time type representing metadata. Now, you
need to access this field or property at runtime. You do this using the <xref:Metalama.Framework.Code.IExpression.Value>
property, which returns an object that, for the code of your template, is of `dynamic` type. When Metalama sees
this `dynamic` object, it replaces it with the syntax representing the field or property, i.e., `this._logger`
or `this.Logger`. So, the line `var logger = (ILogger) loggerFieldOrProperty.Value!;` generates code that stores
the `ILogger` field or property into a local variable that can be used in the runtime part of the template.

### BuildEligibility

The last requirement to implement is to report an error when the aspect is applied to a static method.

We achieve this using the eligibility feature. Instead of manually reporting an error
using `builder.Diagnostics.Report`, the benefit of using this feature is that, with eligibility, the IDE will not even
propose the aspect in a refactoring menu for a target that is not eligible. This is less confusing for the user of the
aspect.

> [!div class="see-also"]
> <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder>
> <xref:template-compile-time>
> <xref:dynamic-typing>
> <xref:overriding-methods>
> <xref:template-parameters>
> <xref:eligibility>



