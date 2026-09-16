---
uid: sample-retry-5
level: 300
summary: "This document explains how to use Polly for advanced retry policies and how to integrate it with Metalama to reduce boilerplate code."
keywords: "Polly, retry policies, boilerplate code, .NET, PolicyKind, IPolicyFactory, resilience framework"
created-date: 2023-04-05
modified-date: 2024-09-09
---

# Retry example, step 5: Using Polly

If you had the feeling that we were reinventing the wheel in the previous Retry examples, you were partially right.
Libraries like [Polly](https://www.pollydocs.org/) offer more advanced and configurable retry strategies, but even
Polly requires some boilerplate code. Wrapping the whole method body in a delegate call and adding proper logging with
parameter values entails boilerplate. With a Metalama aspect, we can completely avoid this boilerplate.

## Infrastructure code

Before jumping into the implementation, let's consider the architecture and infrastructure code.

We want the Polly strategies to be centrally configurable. In a production environment, part of this configuration may be
read from an XML or JSON file. The user of this aspect will only need to specify which _kind_ of strategy is required for
the target method by specifying a `StrategyKind`, a new `enum` we will define. Then the target method will obtain the
resilience pipeline from our `IResiliencePipelineFactory`, which will map the `StrategyKind` to a Polly `ResiliencePipeline` or `ResiliencePipeline<T>` object. The `ResiliencePipelineFactory`
implementation is supplied by the dependency injection framework. Its implementation would typically differ in a testing
or production environment.

### StrategyKind

This `enum` represents the kinds of strategies that are available to business code. You can extend it at will.

[!metalama-file StrategyKind.cs]

We can imagine having several aspect custom attributes for each `StrategyKind`. You can add parameters to strategies, as
long as these parameters can be implemented as properties of a custom attribute.

### IResiliencePipelineFactory

This interface is responsible for returning an instance of the `ResiliencePipeline` or `ResiliencePipeline<T>` class that corresponds to the given `StrategyKind`.

[!metalama-file IResiliencePipelineFactory.cs]

### ResiliencePipelineFactory

Here is a minimalistic implementation of the `IResiliencePipelineFactory` class. You can make it as complex as necessary, but this
goes beyond the scope of this example. This class must be added to the dependency collection.

[!metalama-file ResiliencePipelineFactory.cs]

## Business code

Let's compare the source business code and the transformed business code.

[!metalama-compare RemoteCalculator.cs]

## Aspect implementation

Here is the source code of the `Retry` aspect:

[!metalama-file RetryAttribute.cs]

This aspect pulls in two dependencies, `ILogger` and `IResiliencePipelineFactory`.
The <xref:Metalama.Extensions.DependencyInjection.IntroduceDependencyAttribute?text=[IntroduceDependency]> custom
attribute on top of the fields introduces the fields and pulls them from the constructor.

Polly, by design, requires the logic to be wrapped in a delegate. In a Metalama template, we achieve this only with a
local function because anonymous methods or lambdas can't contain calls to `meta.Proceed`. These local functions are
named `ExecuteVoid`, `ExecuteCore` or `ExecuteCoreAsync`. They have an exception handler that prints a warning with the method name and
all method arguments when the method fails.

We now have the best of both worlds: a fully-featured resilience framework with Polly, and boilerplate reduction with
Metalama.