---
uid: sample-retry-3
level: 200
summary: "This document explains how to handle `CancellationToken` parameters in async methods by adjusting the `OverrideMethodAspect` template to transfer the token to `Task.Delay`."
keywords: "CancellationToken, async methods, handle cancellation tokens, CancellationToken parameter"
created-date: 2023-04-05
modified-date: 2024-09-09
---

# Retry example, step 3: Handling cancellation tokens

[!metalama-project-buttons .]

Our next goal, now that we have appropriately handled async methods, is to handle
the <xref:System.Threading.CancellationToken> parameters. When there is a cancellation token, we should transfer it to
the call to `Task.Delay`.

[!metalama-compare RemoteCalculator.cs ]

## Implementation

In this example, we have adjusted the `OverrideMethodAspect` template in the following way:

[!metalama-file RetryAttribute.cs ]

Let's look at the first lines:

```cs
var cancellationTokenParameter 
    = meta.Target.Parameters.Where( p => p.Type.Is( typeof( CancellationToken ) ) ).LastOrDefault();
```

This code defines a local variable and assigns it to the last parameter of type `CancellationToken`, or to `null` if
there is no such parameter. The expression `meta.Target.Parameters` gives access to the parameters of the method to
which the template is applied. This expression is evaluated at compile time and, by transitivity,
the `cancellationTokenParameter` variable is also defined as a compile-time local variable.

We use this variable to determine which overload of `Task.Delay` to use. The `if` statement is executed at compile time,
as the `cancellationTokenParameter != null` expression is entirely known at compile
time. `cancellationTokenParameter.Value` returns the run-time value of the parameter, which translates
to `cancellationToken` in most cases.

```cs
if ( cancellationTokenParameter != null )
{
      await Task.Delay( (int) delay, cancellationTokenParameter.Value );
}
else
{
      await Task.Delay( (int) delay );
}
```

## Limitations

Our example still has two drawbacks:

1. The logging is too fundamental and hardcoded to `Console.WriteLine`.

> [!div class="see-also"]
> <xref:template-compile-time>
> <xref:dynamic-typing>



