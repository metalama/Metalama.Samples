---
uid: sample-legacy-replacement
level: 300
summary: "This series shows how to replace a component that is called from many places, step by step and without editing the callers, by intercepting the calls with Metalama."
keywords: "replace legacy code, branch by abstraction, interceptors, migration, call counting, parallel run"
created-date: 2026-10-12
modified-date: 2026-10-12
---

# Replacing an implementation without touching its callers

Replacing a component that is called from many places is risky when all the call sites must change at once. A safer approach is _branch by abstraction_: you add an abstraction between the callers and the component, you switch the implementation behind the abstraction step by step, and you remove the abstraction at the end.

In this series, the abstraction is a Metalama interceptor. A fabric rewrites the calls to the legacy component at build time, so the abstraction does not require any change in the calling code. For details about interceptors, see <xref:intercepting-calls>.

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
