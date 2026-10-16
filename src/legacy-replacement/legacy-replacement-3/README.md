---
uid: sample-legacy-replacement-3
level: 300
summary: "This article shows how to redirect the calls of the migrated modules to a new implementation with a tagged fabric query, and how to finish the migration by rewriting the call sites and removing the interceptors."
keywords: "interceptors, tagged query, WithTag, ScopeTag, ForEachSite, branch by abstraction, migration per module, remove interceptors"
created-date: 2026-10-12
modified-date: 2026-10-12
---

# Replacing an implementation, step 3: Switching one module at a time

[!metalama-project-buttons .]

In the [previous step](xref:sample-legacy-replacement-2), we compared the results of the new calculator with the results of the legacy one. In this step, we switch the calls to the new calculator one module at a time. Each module goes through two stages:

1. `Comparing`: the calls go to the legacy calculator, and some of them are compared with the new calculator, as in the previous step.
2. `Migrated`: the calls go to the new calculator.

The modules that are in no stage still call the legacy calculator directly.

## Adapting the calls to the new calculator

The calls in the source code are written for the legacy calculator: their receiver is a `LegacyTaxCalculator` object. The interceptor of the migrated calls receives this object and ignores it:

[!metalama-file NewTaxCalculatorAdapter.cs]

The `TaxComparison` class is the same as in the previous step.

## Selecting the modules with a tagged query

The project fabric declares the stage of each module:

[!metalama-file Fabric.cs marker="Modules"]

The fabric then selects the namespace of each module, tags it with its stage, and registers the interception:

[!metalama-file Fabric.cs marker="Registration"]

* The `SelectMany` query finds the namespace of each module with the <xref:Metalama.Framework.Code.INamespace.GetDescendant*> method.
* The <xref:Metalama.Framework.Fabrics.IQuery`1.WithTag*> method attaches the stage of the module to its namespace.
* The `InterceptMethods` method, called on the tagged query, makes each namespace a scope. Only the calls written in these namespaces are intercepted.
* The delegate given to <xref:Metalama.Extensions.Interceptors.IMethodSelection.ForEachSite*> runs for each call site. It reads the stage of the module through the <xref:Metalama.Extensions.Interceptors.IInterceptionContext.ScopeTag> property, and redirects the call to the matching interceptor.

To move a module to the next stage, you change one line of the fabric. The callers do not change. If the new calculator causes a problem in production, you move the module back to the previous stage.

At build time, Metalama redirects the calls in the `Billing` module to the new calculator:

[!metalama-compare Billing/InvoiceService.cs]

The calls in the `Shipping` module go to the comparison:

[!metalama-compare Shipping/ShippingService.cs]

The calls in the `Reporting` module do not change:

[!metalama-compare Reporting/TaxReport.cs]

## Result

The program produces the following output:

```text
Invoice total in EU: 120.00
Shipping cost in EU: 12.00
Invoice total in US: 110.00
Shipping cost in US: 11.00
Invoice total in CH: 108.10
  Difference at ComputeShippingCost, line 11 for 10.00 in CH: legacy 1.00, new 0.81.
Shipping cost in CH: 11.00
Annual tax in EU: 900.00
```

The invoice total in the `CH` region now uses the tax rate of the new calculator. The shipping cost still uses the legacy calculator, and the difference is logged.

## Finishing the migration

The interceptors are a temporary abstraction. They make the migration safe, but they hide the real behavior of the code: a reader of `InvoiceService` sees a call to the legacy calculator, while the program calls the new one. Do not keep them after the migration.

When all the modules are in the `Migrated` stage, and the new calculator has run in production for long enough, finish the migration:

1. Rewrite the call sites in one reviewed change. Replace each call to `LegacyTaxCalculator.ComputeTax` with a call to `NewTaxCalculator.ComputeTax`, and remove the fields that store a `LegacyTaxCalculator`. The behavior of the program does not change, because the interceptors already redirected these calls. The reviewers only have to check that the change is mechanical.
2. Remove the interceptors: delete the fabric, the `NewTaxCalculatorAdapter` class and the `TaxComparison` class.
3. Delete the `LegacyTaxCalculator` class. If a call to it remains, the build fails, so no call site can be forgotten.

> [!div class="see-also"]
> <xref:interceptors-selecting-calls>
> <xref:sample-legacy-replacement>
