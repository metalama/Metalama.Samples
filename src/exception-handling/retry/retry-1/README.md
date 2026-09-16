---
uid: sample-retry-1
level: 200
summary: "This document introduces a `RetryAttribute` class that retries method execution on exceptions, demonstrating its implementation and limitations."
keywords: "RetryAttribute, exceptions, OverrideMethodAspect, retry behavior, .NET, method retries, exception handling, custom attribute"
created-date: 2023-04-05
modified-date: 2024-09-09
---

# Retry example, step 1: Getting started

[!metalama-project-buttons .]

In this first example, we demonstrate a simple aspect that catches all exceptions and retries the method execution until
the number of attempts reaches a maximum.

Let's see what this aspect does with the following code:

[!metalama-compare RemoteCalculator.cs]

When we call this method, it produces the following output:

```
Trying for the 1-th time.
Operation is not valid due to the current state of the object. Waiting 200 ms.
Trying for the 2-th time.
Operation is not valid due to the current state of the object. Waiting 400 ms.
Trying for the 3-th time.
Operation is not valid due to the current state of the object. Waiting 800 ms.
Trying for the 4-th time.
Succeeded
```

## Implementation

The aspect is implemented by the `RetryAttribute` class.

[!metalama-file RetryAttribute.cs]

The `RetryAttribute` class derives from the <xref:Metalama.Framework.Aspects.OverrideMethodAspect> abstract class, which
in turn derives from the <xref:System.Attribute?text=System.Attribute> class. This makes `RetryAttribute` a custom
attribute.

The `RetryAttribute` class implements the <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideMethod*> method.
This method acts like a _template_. Most of the code in this template is injected into the target method, i.e., the
method to which we add the `[Retry]` custom attribute.

Inside the <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideMethod*> implementation, the call
to `meta.Proceed()` has a special meaning. When the aspect is applied to the target, the call to `meta.Proceed()` is
replaced by the original implementation, but with a few syntactic changes to capture the return value. We
colored `meta.Proceed()` differently than the other code to remind you that it has a special meaning. If you use
Metalama Tools for Visual Studio, you will also enjoy syntax highlighting in this IDE.

To implement the retry behavior, we wrap the call to `meta.Proceed()` within a `for` loop and `try...catch` exception
handler.

The `RetryAttribute` class has two properties: `Delay` and `Attempts`. The value of these properties is used in
the <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideMethod*> implementation. Because the value of these
properties is known and compile time, it will replace them with their value in the template.

## Limitations

This first example has severe limitations:

* The `async` variant should use `Task.Delay` instead of `Thread.Sleep`.
* The logging is too basic and hardcoded to `Console.WriteLine`.

We will address these limitations in the following examples.

> [!div class="see-also"]
> <xref:simple-override-method>
> <xref:quickstart-adding-aspects>
  

  



