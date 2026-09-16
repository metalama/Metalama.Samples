---
uid: sample-enrich-exception
level: 200
summary: "This document demonstrates how to enrich exceptions with parameter values, enhancing call stacks for better debugging and context."
keywords: "exception handling, call stacks, debugging, parameter values, .NET, enrich exceptions, context information, exception handler, crash report"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Example: Enriching exceptions with parameter values

[!metalama-project-buttons .]

Call stacks are essential for diagnosing software issues; however, without parameter values, they can be less helpful.
They lack context by only showing the sequence of method calls, which fails to reveal the data being processed during
execution. Parameter values give you insight into the application's state, which is vital to pinpoint the root cause
easily. Without parameter values, recreating an issue becomes time-consuming and increases the tedium of tracing through
code paths since you cannot directly assess the impact of input data on the error from the call stack alone.

In this example, we will show how to include parameter values into the call stack. The idea is to add an exception
handler to all non-trivial methods, to append context information to the current <xref:System.Exception>, and rethrow
the exception to the next stack frame.

Here is an example of a method that has an exception handler:

[!metalama-compare Calculator.cs]

In the last-chance exception handler, you can now include the context information in the crash report:

[!metalama-file Program.cs]

This program produces the following output:

```text
System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values. (Parameter 'n')
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 8
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Calculator.Fibonaci(Int32 n) in Calculator.cs:line 23
   at Program.Main() in Program.cs:line 7
---with---
Calculator.Fibonaci(-1)
Calculator.Fibonaci(1)
Calculator.Fibonaci(2)
Calculator.Fibonaci(3)
Calculator.Fibonaci(4)
Calculator.Fibonaci(5)
----------
```

As you can see, parameter values are now included in the crash report.

## Infrastructure code

Let's see how it works.

The exception handler calls the helper class `EnrichExceptionHelper`:

[!metalama-file EnrichExceptionHelper.cs]

## Aspect code

The `EnrichExceptionAttribute` aspect is responsible for adding the exception handler to methods:

[!metalama-file EnrichExceptionAttribute.cs]

Most of the code in this aspect builds an interpolated string, including the method name and its parameters. We have
commented on this technique in detail in <xref:sample-log-3>.

## Fabric code

Adding the aspect to all methods by hand as a custom attribute would be highly cumbersome. Instead, we are using a
fabric that adds the exception handler to all public methods of all public types. Note that we exclude the `ToString`
method to avoid infinite recursion. For the same reason, we have excluded the aspect from
the `EnrichExceptionHelper.AppendContextFrame` method.

[!metalama-file Fabric.cs]

> [!WARNING]
> Including sensitive information (e.g., user credentials, personal data, etc.) in logs can pose a security risk. Be
> cautious when adding parameter values to logs and avoid exposing sensitive data.
> To remove sensitive information from the logs, see <xref:sample-log-7>



