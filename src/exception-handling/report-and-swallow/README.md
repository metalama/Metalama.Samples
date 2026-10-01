---
uid: sample-report-and-swallow
level: 200
summary: "This document explains the _Report and Swallow_ exception-handling strategy for last-chance exceptions in plug-in applications, utilizing the `ReportAndSwallowExceptions` aspect to automate exception handling."
keywords: "exception-handling strategy, last-chance exceptions, `try...catch` block, `ILastChanceExceptionHandler`"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Example: Report and swallow exceptions

[!metalama-project-buttons .]

_Report and swallow_ is a _last-chance_ exception-handling strategy, i.e. a strategy for exceptions that could not be
handled deeper in the call stack. In general, swallowing a last-chance exception is not a good idea because it may leave
your application in an invalid state. Also, using an aspect to implement a last-chance exception handler is generally
not a good idea because it is simpler to use
the <xref:System.AppDomain.UnhandledException?text=AppDomain.CurrentDomain.UnhandledException> event.

However, there are some cases where you need to handle last-chance exceptions with a `try...catch` block. For instance,
when your code is a _plug-in_ in some host application like Office or Visual Studio, exceptions that are not handled by
Visual Studio Extensions have a chance to crash the whole Visual Studio without any error message.

In this case, you must implement exception handling at each entry point of your API. This includes:

* any interface of the host that your plug-in implements,
* any handlers to host events your plug-in subscribes to,
* any Winforms/WPF entry point.

Since there can be hundreds of such entry points, it is useful to automate this pattern using an aspect.

In this example, we will assume that we have a host application that defines an interface `IPartProvider` that plug-ins
can implement. Let's see what the aspect does with the code:

[!metalama-compare PartProvider.cs]

As we can see, the `ReportAndSwallowExceptions` aspect pulls the `ILastChanceExceptionHandler` from the dependency
injection container and adds a `try...catch` block to the target method.

## Infrastructure code

The aspect uses the following interface:

[!metalama-file ILastChanceExceptionHandler.cs]

## Aspect code

The `ReportAndSwallowExceptionsAttribute` aspect is rather simple:

[!metalama-file ReportAndSwallowExceptionsAttribute.cs]

If you have read the previous examples, the following notes should be redundant.

The `ReportAndSwallowExceptionsAttribute` class derives from the <xref:Metalama.Framework.Aspects.OverrideMethodAspect>
abstract class, which in turn derives from the <xref:System.Attribute?text=System.Attribute> class. This
makes `ReportAndSwallowExceptionsAttribute` a custom attribute.

The `ReportAndSwallowExceptionsAttribute` class implements
the <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideMethod*> method. This method acts like a _template_.
Most of the code in this template is injected into the target method, i.e., the method to which we add
the `[ReportAndSwallowExceptionsAttribute]` custom attribute.

Inside the <xref:Metalama.Framework.Aspects.OverrideMethodAspect.OverrideMethod*> implementation, the call
to `meta.Proceed()` has a very special meaning. When the aspect is applied to the target, the call to `meta.Proceed()`
is replaced by the original implementation, with a few syntactic changes to capture the return value.

To remind you that `meta.Proceed()` has a special meaning, it is colored differently than the rest of the code. If you
use Metalama Tools for Visual Studio, you will also enjoy syntax highlighting in this IDE.

Around the call to `meta.Proceed()`, we have a `try...catch` exception handler. The `catch` block has a `when` clause
that should ensure that we do not handle exceptions that are accepted by the host,
typically <xref:System.OperationCanceledException>. Then, the `catch` handler simply reports the exception and does not
rethrow it.

The <xref:Metalama.Extensions.DependencyInjection.IntroduceDependencyAttribute?text=[IntroduceDependency]> on the top of
the `_exceptionHandler` field does the magic of introducing the field and pulling it from the constructor.

> [!div class="see-also"]
> <xref:simple-override-method>
> <xref:quickstart-adding-aspects>
> <xref:dependency-injection>



