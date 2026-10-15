---
uid: sample-call-telemetry-1
level: 300
summary: "This example audits every call to File.Delete and Process.Start in a project, by redirecting the calls to static methods that receive the caller through caller information attributes."
keywords: "interceptors, audit, sensitive API, File.Delete, Process.Start, RedirectToExistingMethod, ForEachSite, ExcludeInterceptors, CallerMemberName, CallerFilePath, CallerLineNumber, project fabric"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Call telemetry example, step 1: Auditing calls to sensitive APIs

[!metalama-project-buttons .]

Some APIs require an audit trail. For instance, a security policy may require that every deletion of a file and every
start of a process is recorded, with the code that requested it. The audit must cover the calls that the team adds
later, and must not depend on each developer remembering to write an audit statement.

In this example, a project fabric redirects every call to `File.Delete` and `Process.Start` in the project to static
methods of an `Audit` class. These methods record the call, then call the original API. The calling code does not
change in the source code:

[!metalama-compare BackupService.cs]

When the program runs, the audit methods write the following lines. The paths change from one run to another.

```
[audit] BackupService.cs, line 10, Backup: Process.Start tar -cf C:\Temp\call-telemetry-xrrp4jza.nqv\reports.tar -C C:\Temp\call-telemetry-xrrp4jza.nqv\reports .
[audit] BackupService.cs, line 15, Backup: File.Delete costs.csv
[audit] BackupService.cs, line 15, Backup: File.Delete sales.csv
```

## The audit methods

The `Audit` class is plain C# code. You can debug it and unit-test it like any other class:

[!metalama-file Audit.cs]

Each audit method has the same parameters as the API that it audits, followed by parameters with a caller information
attribute: `[CallerMemberName]`, `[CallerFilePath]` and `[CallerLineNumber]`. Metalama passes the location of the
intercepted call site to these parameters, not the location of the call inside the audit method.

The audit methods call `File.Delete` and `Process.Start` themselves. Without precaution, these calls would also be
intercepted, and the audit methods would call themselves. The `[ExcludeInterceptors]` attribute on the `Audit` class
prevents this: the calls in this class are never intercepted.

## The fabric

The following project fabric registers the interceptors:

[!metalama-file AuditFabric.cs]

`File.Delete` has a single overload, so the first registration redirects all its calls to `Audit.DeleteFile` with
`RedirectToExistingMethod`.

`Process.Start` has several overloads, and an existing method can intercept only an overload whose arguments it
receives. The second registration therefore decides for each call site with `ForEachSite`. It looks for the
`StartProcess` method that receives every argument of the called overload, by name and type. When no such method
exists, for instance for `Process.Start( string )`, the fabric reports the error `AUDIT01` instead of leaving the call
unchanged. A call that the audit cannot record therefore breaks the build, and the developer adds an audit method for
this overload.

Calls to other APIs, such as `Directory.Delete` or `FileInfo.Delete`, are not audited. To audit them, add a method to
the `Audit` class and a registration to the fabric.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:redirecting-calls-to-existing-methods>
> <xref:interceptors-selecting-calls>
