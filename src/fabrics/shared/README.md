---
uid: sample-shared-fabric
summary: "This document is an example by Whit Waldo demonstrating shared fabrics, including projects with logging aspects and a project fabric for applying these aspects."
keywords: "shared fabrics, project fabric, Metalama aspects, .NET, shared aspects, subdirectory, GitHub"
created-date: 2024-09-09
modified-date: 2024-09-09
---

# Example: shared fabrics

This example has been kindly contributed by [Whit Waldo](https://github.com/WhitWaldo).

It demonstrates the techniques explained in <xref:fabrics-many-projects>. It is best viewed in GitHub, or when locally, rather than in the documentation browser.

It contains the following files and directories:

* `SharedAspects/` is a project containing shared Metalama aspects, in this case, logging.
* `Models/` and `App/` are two projects that must be logged.
* `SharedFabrics.cs` is a project fabric that adds the logging aspect to all public and internal classes of the current project.
* `Directory.Build.props` includes `SharedFabrics.cs` in all projects in any subdirectory.






