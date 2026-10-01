---
uid: sample-log
summary: "This document outlines a series of examples for implementing logging in application development, covering aspects from basic logging to advanced features like dependency injection and data redaction."
keywords: "logging, aspect, ILogger, tracing"
created-date: 2023-03-29
modified-date: 2024-09-09
---

# Implementing logging without boilerplate

Logging is a key aspect of application development, enabling developers to understand the execution flow and identify bottlenecks. By providing detailed insights into system interactions, tracing helps optimize performance, pinpoint issues, and improve user experience. It plays a significant role in ensuring efficient and reliable software.

## Objective

This series of examples shows how to build an aspect that automatically writes a log record when a method execution starts, completes, or fails.

The objective is to transform the code as illustrated in the following example. If the generated code does not fit entirely to your preferences, do not worry, you can learn how to personalize it.

[!metalama-compare log-4/Calculator.cs]

## In this series

We start with the most trivial implementation and progressively add new features:

| Article | Description |
|--------|-------------|
| [Logging a constant message](log-1/README.md) | This is the simplest possible implementation of logging. |
| [Adding the method name](log-2/README.md) | Instead of logging a constant, generic message, we now include the method name in the message. |
| [Adding the parameter values](log-3/README.md) | We now add the parameter values and the return value to the message. |
| [Using to ILogger](log-4/README.md) | Instead of using `Console.WriteLine`, we inject an `ILogger` into the target type using dependency injection. |
| [ILogger without dependency injection](log-5/README.md) | Instead of using dependency injection, we expect the source code to contain an `ILogger` field and report errors if it does not. |
| [Adding logging to many methods](log-6/README.md) | So far, we have manually added a custom attribute to each method. In this example, we show how to target several methods programmatically using compile-time code queries.
| [Redacting sensitive data](log-7/README.md) | Passwords and other sensitive data are excluded from the log. |
| [Avoiding infinite recursions](log-8/README.md) | Adds a recursion guard to avoid infinite recursions due to logging. |


