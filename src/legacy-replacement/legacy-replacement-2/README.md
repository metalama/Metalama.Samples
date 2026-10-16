---
uid: sample-legacy-replacement-2
level: 300
summary: "This article shows how to run a new implementation next to a legacy one for a sample of the calls, log the differences, and keep the result of the legacy implementation, without editing the calling code."
keywords: "interceptors, parallel run, compare implementations, sampling, legacy code, RedirectToExistingMethod, ExcludeInterceptors"
created-date: 2026-10-12
modified-date: 2026-10-12
---

# Replacing an implementation, step 2: Comparing the new implementation with the old one

[!metalama-project-buttons .]

In the [previous step](xref:sample-legacy-replacement-1), we found which calls to the legacy calculator actually run. Before we switch these calls to the new calculator, we want evidence that the new calculator returns the same results. In this step, we call both calculators for a sample of the calls, log the differences, and return the result of the legacy calculator. The behavior of the program does not change.

## The new calculator

The new calculator has a different design: it is a static class. It also supports a new region, `CH`, with a different tax rate.

[!metalama-file NewTaxCalculator.cs]

## Comparing the results

The `TaxComparison` class has a static method with the same parameters as `LegacyTaxCalculator.ComputeTax`. It also receives the calculator and the location of the call site:

[!metalama-file TaxComparison.cs marker="Compare"]

The method always calls the legacy calculator and returns its result. Only one call out of three also calls the new calculator. This sampling limits the cost of the comparison when the component is called often. When the results differ, or when the new calculator throws an exception, the method logs the difference with the location of the call site. An exception in the new calculator does not affect the caller.

As in the previous step, the class has the <xref:Metalama.Extensions.Interceptors.ExcludeInterceptorsAttribute> attribute, so the call to the legacy calculator in the interceptor is not intercepted.

## Redirecting the calls

The project fabric is the same as in the previous step, except that it gives the `TaxComparison.ComputeTax` method:

[!metalama-file Fabric.cs]

## Result

The program produces the following output:

```text
Invoice total in EU: 120.00
Shipping cost in EU: 12.00
Invoice total in US: 110.00
Shipping cost in US: 11.00
Invoice total in CH: 110.00
  Difference at ComputeShippingCost, line 11 for 10.00 in CH: legacy 1.00, new 0.81.
Shipping cost in CH: 11.00
Annual tax in EU: 900.00
```

The results of the program are the same as with the legacy calculator. The log shows that the calculators differ for the `CH` region. You can now decide whether the difference is a defect of the new calculator or a correction of the legacy one.

> [!div class="see-also"]
> <xref:redirecting-calls-to-existing-methods>
> <xref:sample-legacy-replacement-3>
