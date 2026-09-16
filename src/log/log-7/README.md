---
uid: sample-log-7
level: 300
summary: "This document explains how to prevent logging sensitive data by filtering parameters and using custom attributes, with examples and implementation details."
keywords: "sensitive data, logging sensitive data, prevent logging, custom attributes, .NET, parameter filtering"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Logging example, step 7: Removing sensitive data

[!metalama-project-buttons .]

Ensuring the security and privacy of sensitive data is a critical responsibility for developers. Logs can inadvertently
expose sensitive information, especially if all methods and their parameter values are logged without review. This
example shows how to prevent logging specific parameters, mitigating the risk of data breaches and unauthorized access.

In the following examples, the password and the salt are excluded from the log:

[!metalama-compare LoginService.cs]

## Implementation

Parameters are filtered by the following compile-time class:

[!metalama-file SensitiveParameterFilter.cs]

As shown, the `IsSensitive` method must determine whether a parameter is sensitive. It bases its decision on two
factors: if the parameter name contains well-known keywords or if the parameter is explicitly annotated with
the `[NotLogged]` custom attribute, which we just defined for this project.

[!metalama-file NotLoggedAttribute.cs]

The `LogAttribute` aspect has been modified to call `SensitiveParameterFilter.IsSensitive` and use the text `<redacted>`
instead of the parameter value for sensitive parameters.

[!metalama-file LogAttribute.cs]

> [!WARNING]
> This approach does not guarantee that there will be no leak of sensitive data to logs because it relies on manual
> identification of parameters by you, the aspect's developer, or by the aspect's users, which is subject to human
> error.
> To verify that you have not forgotten anything, consider the following strategies:
>
>  * Do not pass sensitive data in strings, but wrap them into an object and do not expose sensitive data in the
     implementation of the `ToString` method of this wrapping class.
>  * Perform tests by injecting well-known strings as values for sensitive parameters (e.g., `p@ssw0rd`), enable logging
     to the maximum verbosity, and verify that the logs do not contain any of the well-known values. These tests must
     have complete coverage to be accurate.



