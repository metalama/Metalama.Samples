---
uid: sample-retry-4
level: 300
summary: "This document describes adding real-world logging to a retry mechanism, including code updates and warnings about logging sensitive information."
keywords: "logging, retry"
created-date: 2023-04-05
modified-date: 2024-09-09
---

# Retry example, step 4: Adding logging

[!metalama-project-buttons .]

In this article, we will add real-world logging to the retry aspect. To learn more about logging, please refer
to <xref:sample-log>.

Below is the updated code transformation:

[!metalama-compare RemoteCalculator.cs ]

The code now produces the following output:

```
Trying for the 1-th time.
warn: RemoteCalculator[0]
      RemoteCalculator.Add(a = {1}, b = {1}) has failed: Operation is not valid due to the current state of the object. Retrying in 200 ms.
trce: RemoteCalculator[0]
      RemoteCalculator.Add(a = {1}, b = {1}): retrying now.
Trying for the 2-th time.
warn: RemoteCalculator[0]
      RemoteCalculator.Add(a = {1}, b = {1}) has failed: Operation is not valid due to the current state of the object. Retrying in 400 ms.
trce: RemoteCalculator[0]
      RemoteCalculator.Add(a = {1}, b = {1}): retrying now.
Trying for the 3-th time.
warn: RemoteCalculator[0]
      RemoteCalculator.Add(a = {1}, b = {1}) has failed: Operation is not valid due to the current state of the object. Retrying in 800 ms.
trce: RemoteCalculator[0]
      RemoteCalculator.Add(a = {1}, b = {1}): retrying now.
Trying for the 4-th time.
Succeeded
```

> [!WARNING]
> Be careful when including sensitive information (e.g., user credentials, personal data, etc.) in logs as they can pose
> a security risk. Avoid exposing sensitive data and remove them from logs using techniques such as <xref:sample-log-7>.

## Implementation

To improve logging, we included the method name and parameter values in the logging message using
the `LoggingHelper.BuildInterpolatedString` method. For more information about the method, refer to <xref:sample-log-2>
and <xref:sample-log-3>.

To substitute `Console.WriteLine` with `ILogger` and inject the `ILogger` into the target class, we used dependency
injection. Refer to <xref:sample-log-4> for details.

Below is the aspect code update:

[!metalama-file RetryAttribute.cs ]

> [!div class="see-also"]
> <xref:sample-log-2>
> <xref:sample-log-3>
> <xref:sample-log-4>
> <xref:template-overview>
> <xref:Metalama.Framework.Code.SyntaxBuilders.InterpolatedStringBuilder>
> <xref:template-compile-time>
> <xref:dynamic-typing>



