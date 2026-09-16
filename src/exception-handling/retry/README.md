---
uid: sample-retry
summary: "How to build an aspect that automatically retries a method after a transient failure, with async support, logging, and a Polly-based variant."
keywords: "retry mechanisms, C#, .NET, transient faults, exponential backoff, automatic retries, async support, logging, Polly"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Sample: retry

Calls to a database or a remote service can fail for reasons that go away on their own: a congested network, a brief outage, a lock timeout. Retrying the call, typically with exponential backoff, is often enough to recover from these transient faults.

This series of articles describes how to construct an aspect that automatically retries a failed method. This aspect modifies a method in the following way:

[!metalama-compare retry-4/RemoteCalculator.cs]

## In this series

We start with the most basic implementation and add features progressively.

| Description | Article |
|-------------|---------|
| <xref:sample-retry-1> | This is the most basic retry aspect. |
| <xref:sample-retry-2> | In this example, we add support for `async` methods and call `await Task.Delay` instead of `Thread.Sleep`. |
| <xref:sample-retry-3> | Here, we add support for `CancellationToken` parameters, which we pass to `Task.Delay`. |
| <xref:sample-retry-4> | We now add proper logging using `ILogger` and dependency injection. |
| <xref:sample-retry-5> | Finally, we show how to use Polly instead of our custom and naïve implementation of the retry logic. |



