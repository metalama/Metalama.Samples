---
uid: samples-exception-handling
summary: "This document provides examples of exception handling, covering retry mechanisms, enriching exceptions for debugging, and reporting/swallowing last-chance exceptions."
keywords: "exception handling, retry mechanisms, enriching exceptions, debugging, reporting exceptions, swallowing exceptions, .NET, retry execution, exception object, crash reports"
created-date: 2023-04-06
modified-date: 2024-09-09
---

# Examples: exception handling

This section gives several examples of exception-handling aspects:

| Article | Description |
|--------|--------|
| [Retry](retry/README.md) | This series of articles show how to retry the execution of a method when it fails with an exception. Each article adds more features and grows in complexity. It culminates in the integration of Polly. |
| [Enriching exceptions](enrich-exception/README.md) | This article shows how to add the parameter values to the exception object, and create crash reports that are easier to debug.
| [Report and swallow](report-and-swallow/README.md) | This article shows how to build an aspect that reports and swallows (ignores) last-chance exception, a pattern that is sometimes useful when building plug-ins, at the entry point of the plug-in code. |


