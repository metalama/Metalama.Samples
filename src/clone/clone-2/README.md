---
uid: sample-clone-2
summary: "This document discusses improving a Deep Clone aspect by adding error reporting for unsupported situations like read-only fields, non-cloneable types, and non-automatic properties."
keywords: "Deep Clone, Clone method"
created-date: 2023-04-21
modified-date: 2024-09-09
---

# Clone example, step 1: verifying code

[!metalama-project-buttons .]

In the previous article, we built an aspect that implements the Deep Clone pattern. The aspect assumes that all fields
or properties annotated with the `[Child]` attribute are both of a cloneable type and assignable. If this is not the
case, the aspect generates uncompilable code, making the aspect's user confused.

In this article, we will improve the aspect so that it reports errors in the following three unsupported situations.

We report an error when the field or property is read-only.

[!metalama-test ../../tests/Metalama.Samples.Clone2.Tests/ErrorReadOnlyField.cs]

We report an error when the type of the field or property does not have a `Clone` method.

[!metalama-test ../../tests/Metalama.Samples.Clone2.Tests/ErrorNotCloneableChild.cs]

The `Clone` method must be `public` or `internal`.

[!metalama-test ../../tests/Metalama.Samples.Clone2.Tests/ErrorProtectedCloneMethod.cs]

We report an error when the property is not an automatic property.

[!metalama-test ../../tests/Metalama.Samples.Clone2.Tests/ErrorNotAutomaticProperty.cs]

## Aspect implementation

The full updated aspect code is here:

[!metalama-file CloneableAttribute.cs]

The first thing to do is to define the errors we want to report as static fields.

[!metalama-file CloneableAttribute.cs marker="DiagnosticDefinitions"]

For details about reporting errors, see <xref:diagnostics>.

Then, we add the `VerifyFieldsAndProperties` method and call it from `BuildAspect`.

[!metalama-file CloneableAttribute.cs member="CloneableAttribute.VerifyFieldsAndProperties"]

When we detect an unsupported situation, we report the error using
the <xref:Metalama.Framework.Diagnostics.ScopedDiagnosticSink.Report*> method. The first argument is the diagnostic
constructed from the definition stored in the static field, and the second is the invalid field or property.

The third verification requires additional discussion. Our aspect requires the type of child fields or properties to
have a `Clone` method. This method can be defined in three ways: in source code (i.e., hand-written), in a referenced
assembly (compiled), or introduced by the `Cloneable` aspect itself. In the latter case, the `Clone` method may not yet
be present in the code model because the child field type may not have been processed yet. Therefore, if we don't find
the `Clone` method, we should check if the child type has the `Cloneable` aspect. This aspect can be added as a custom
attribute which we could check using the code model, but it could also be added as a fabric without the help of a custom
attribute. Thus, we must check the presence of the aspect, not the custom attribute. You can check the presence of the
aspect using `fieldType.Enhancements().HasAspect<CloneableAttribute>()`. The problem is that, at design time (inside the
IDE), Metalama only knows aspects applied to the current type and its parent types. Metalama uses that strategy for
performance reasons to avoid recompiling the whole assembly at each keystroke. Therefore, that verification cannot be
performed at design time and must be skipped.

## Summary

Instead of generating invalid code and confusing the user, our aspect now reports errors when it detects unsupported
situations. It still lacks a mechanism to support anomalies. What if the `Game` class includes a collection of `Player`s
instead of just one?

> [!div class="see-also"]
> <xref:diagnostics>



