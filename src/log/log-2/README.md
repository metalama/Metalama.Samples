---
uid: sample-log-2
level: 200
summary: "This document describes enhancing a logging aspect to include method names and signatures, improving its utility across multiple methods."
keywords: "logging aspect"
created-date: 2023-03-28
modified-date: 2024-09-09
---

# Logging example, step 2: Adding the method name

[!metalama-project-buttons .]

In the previous example, a generic message was logged when a method started. This was not very useful when the aspect
was applied to multiple methods. In this example, the aspect is improved to include the name and signature of the
executed method.

The logging aspect generates the _green_ code in the snippet below:

[!metalama-compare Calculator.cs]

## Implementation

The aspect at work is the following:

[!metalama-file LogAttribute.cs]

The `meta.Target.Method` expression is used in an interpolated string to get the method name and signature. The `meta`
pseudo-keyword means that the expression on the right side is evaluated at compile-time.

> [!div class="see-also"]
> <xref:template-overview>



