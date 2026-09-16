---
uid: sample-log-3
level: 250
summary: "This document explains how to enhance logging by including runtime parameter values and return values, while cautioning against logging sensitive information."
keywords: "logging, runtime parameter values, return values"
created-date: 2023-03-29
modified-date: 2024-09-09
---

# Logging example, step 3: Adding parameters values

[!metalama-project-buttons .]

Up until now, our logging aspect writes messages that include _constant_ text and _compile-time_ expressions. Let's now
introduce the values of parameters and the method return value, which are known at _run time_.

It's important to include parameter values in traces because they offer valuable context to help developers comprehend
the application's state during execution. With this contextual information, you can diagnose and debug problems more
easily, decreasing the time spent recreating issues and tracing through code paths, resulting in a more stable and
reliable application.

The code with the transformation from the new aspect can be seen below:

[!metalama-compare Calculator.cs ]

> [!WARNING]
> Adding sensitive information such as user credentials, personal data, etc., to logs can pose a security risk. Exercise
> caution when adding parameter values to logs and avoid exposing sensitive data.
> To remove sensitive information from the logs, see <xref:sample-log-7>

## Implementation

The aspect is as follows:

[!metalama-file LogAttribute.cs]

As you can see, the aspect's code is much more complex.

The most straightforward approach to generate an interpolated string from an aspect is to use
the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder> class and to add literal parts, known at
compile time and constant at run time, and run-time expressions, unknown at compile time.

The `BuildInterpolatedString` method of the aspect class is responsible for constructing
the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder>. Please note that `BuildInterpolatedString`
is _not_ a template method. It is a method that executes entirely at compile time. It has an `includeOutParameters`
parameter that determines if the values of the `out` parameters are available when the interpolated string is in use.

* Firstly, `BuildInterpolatedString` appends the name of the current type and method using
  the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder.AddText*> method. Then, it iterates through
  the collection of parameters of the current method. This collection is available on the
  expression `meta.Target.Parameters`.

* In the `foreach` loop, `BuildInterpolatedString` method checks if the parameter is `out`. If `includeOutParameters`
  parameter is `false`, the method appends a constant text. However, if the parameter can be read, the method adds an
  expression using the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder.AddExpression*> method.

Below is the `OverrideMethod` method. As with previous examples, this method is a template containing both run-time and
compile-time code.

* Firstly, `OverrideMethod` calls `BuildInterpolatedString` to get
  the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder>. Note that the interpolated string created
  by `BuildInterpolatedString` only includes the name and parameters of the method. We still want to append more
  information to the strings, such as the text `started`, `succeeded`, `returned` or `failed`.

* Then, we write the entry message. The aspect calls
  the <xref:Metalama.Framework.Code.SyntaxBuilders.ExpressionBuilderExtensions.ToValue*> method to get the interpolated
  string from the <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder> and passes it
  to `Console.WriteLine`. Note that
  the <xref:Metalama.Framework.Code.SyntaxBuilders.ExpressionBuilderExtensions.ToValue*> method does not really return
  an interpolated string but returns a run-time object of `dynamic` type that, when used in a run-time context, expands
  into the interpolated string.

* Finally, we write the success message. We want to write a different message when the method is `void` than when it
  returns a value. We implement this choice with the `if` in the method. As the `if`
  condition `meta.Target.Method.ReturnType.Is(typeof(void))` is a compile-time expression, Metalama interprets the
  entire `if` at compile time. Notice the specific background color of the compile-time.

> [!div class="see-also"]
> <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder>
> <xref:template-compile-time>
> <xref:dynamic-typing>



