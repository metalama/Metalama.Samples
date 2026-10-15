---
uid: sample-legacy-replacement
level: 300
summary: "This series shows how to replace a component that is called from many places, step by step and without editing the callers, by intercepting the calls with Metalama."
keywords: "replace legacy code, branch by abstraction, interceptors, migration, call counting, parallel run"
created-date: 2026-10-12
modified-date: 2026-10-15
---

# Replacing an implementation without touching its callers

Replacing a component that is called from many places is risky when all the call sites must change at once. A safer approach is _branch by abstraction_: you add an abstraction between the callers and the component, you switch the implementation behind the abstraction step by step, and you remove the abstraction at the end.

In this series, the abstraction is a Metalama interceptor. A fabric rewrites the calls to the legacy component at build time, so the abstraction does not require any change in the calling code. For details about interceptors, see <xref:intercepting-calls>.

## Background

### Branch by abstraction

Paul Hammant described branch by abstraction in 2007, in the article [Introducing Branch by Abstraction](https://paulhammant.com/blog/branch_by_abstraction.html). He credits Stacy Curl with the name. Martin Fowler describes the technique in [BranchByAbstraction](https://martinfowler.com/bliki/BranchByAbstraction.html). Jez Humble and David Farley also describe it in their book _Continuous Delivery_ (Addison-Wesley, 2010).

The technique is an alternative to a long-lived source control branch. It has the following steps:

1. Add an abstraction layer between the callers and the component to replace, and change the callers so that they use the abstraction.
2. Implement the new component behind the abstraction.
3. Switch the callers from the old component to the new one, in small steps. The abstraction decides which implementation each caller uses.
4. When no caller uses the old component, remove the old component and, optionally, the abstraction.

All the changes are made on the main branch. After each step, the code builds and the tests pass. This approach has the following benefits:

- There is no long-lived branch to merge at the end of the migration.
- Each change is small, so it is easier to review and to revert.
- The application can be released at any time during the migration.
- The new component runs in production for some callers first, so a defect affects a limited part of the application.

Two related techniques apply the same principle at a different level. The [strangler fig application](https://martinfowler.com/bliki/StranglerFigApplication.html) replaces a whole system part by part, by routing the requests at the boundary of the system. [Parallel change](https://martinfowler.com/bliki/ParallelChange.html), also called expand and contract, changes an interface in three phases: add the new interface, migrate the callers, then remove the old interface.

### Interceptors as seams

In the classic form of branch by abstraction, the first step edits every call site to use the abstraction. When the component has many callers, this first step is itself the large and risky change that we want to avoid.

In his book _Working Effectively with Legacy Code_ (Prentice Hall, 2004), Michael Feathers defines a seam as "a place where you can alter behavior in your program without editing in that place". A Metalama interceptor creates such a seam at build time. The fabric selects the call sites, and Metalama redirects them to the interceptor during the build. The source code of the callers does not change. When the migration is complete, you rewrite the call sites in one reviewed change, and then you remove the interceptors.

### Comparing two implementations

The comparison of step 2 follows the approach of GitHub's [Scientist](https://github.com/github/scientist) library, described as "a library for carefully refactoring critical paths". Scientist runs the existing code, called the control, and the new code, called the candidate. It compares their results, records the differences and the exceptions of the candidate, and always returns the result of the control. [Scientist.NET](https://github.com/scientistproject/Scientist.net) is a port of this library to .NET. With Scientist, you must wrap each call site in an experiment. With an interceptor, the comparison applies to all the selected call sites without changing them.

## The example

The example application computes taxes with a `LegacyTaxCalculator` class. Three modules call it: the `Billing`, `Shipping` and `Reporting` namespaces. A `NewTaxCalculator` class must replace it.

## In this series

| Article | Description |
|---------|-------------|
| [Finding which calls are actually used](legacy-replacement-1/README.md) | We count the calls to the legacy calculator for each source location, and print the counts when the program exits. |
| [Comparing the new implementation with the old one](legacy-replacement-2/README.md) | We call both calculators for a sample of the calls, log the differences, and keep the result of the legacy calculator. |
| [Switching one module at a time](legacy-replacement-3/README.md) | We redirect the calls of the migrated modules to the new calculator, and explain how to finish the migration. |

> [!div class="see-also"]
> <xref:intercepting-calls>
> <xref:interceptors-use-cases>
