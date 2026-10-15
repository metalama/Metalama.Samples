---
uid: sample-legacy-replacement-1
level: 300
summary: "This article shows how to count the calls to a legacy component for each source location, with an interceptor that receives caller information attributes, without editing the calling code."
keywords: "interceptors, count calls, caller information, CallerFilePath, CallerLineNumber, legacy code, RedirectToExistingMethod, ExcludeInterceptors"
created-date: 2026-10-12
modified-date: 2026-10-12
---

# Replacing an implementation, step 1: Finding which calls are actually used

[!metalama-project-buttons .]

Before you replace a component, you need to know how it is used. A text search finds the call sites in the source code, but it does not tell you which of them actually run. In this step, we count the calls to the legacy calculator for each source location, and print the counts when the program exits.

## The legacy calculator and its callers

The legacy calculator has one method:

[!metalama-file LegacyTaxCalculator.cs]

Three modules call it. For instance, the `Billing` module:

[!metalama-file Billing/InvoiceService.cs]

The `Shipping` and `Reporting` modules are similar. The program calls some of their methods:

[!metalama-file Program.cs]

## Counting the calls

The `CallCounter` class has a static method with the same parameters as `LegacyTaxCalculator.ComputeTax`, and a first parameter that receives the calculator on which the method is called. Its last two parameters have caller information attributes. When Metalama redirects a call to this method, these parameters receive the calling member and the line of the original call site, not of the call inside the interceptor.

[!metalama-file CallCounter.cs]

You can also add a parameter with the `[CallerFilePath]` attribute. Note that it receives the absolute path of the source file on the build machine, and that this path is stored in the binary.

The method counts the call, then calls the legacy calculator, so the behavior of the program does not change. The static constructor registers a handler of the `AppDomain.ProcessExit` event, which prints the counts when the program exits.

The class has the <xref:Metalama.Extensions.Interceptors.ExcludeInterceptorsAttribute> attribute, so the calls in this class are not intercepted. Without this attribute, the call to `calculator.ComputeTax` in the interceptor would be redirected to the interceptor itself.

## Redirecting the calls

The following project fabric redirects all the calls to `LegacyTaxCalculator.ComputeTax` in the project to `CallCounter.ComputeTax`:

[!metalama-file Fabric.cs]

The <xref:Metalama.Extensions.Interceptors.InterceptionAdviserExtensions.InterceptMethods*> method selects the intercepted method by its type and its name. The <xref:Metalama.Extensions.Interceptors.IMethodResultFactory.RedirectToExistingMethod*> method gives the interceptor.

The source code of the callers does not change. At build time, Metalama rewrites the call sites:

[!metalama-compare Billing/InvoiceService.cs]

## Result

The program produces the following output:

```text
Invoice total in EU: 120.00
Shipping cost in EU: 12.00
Invoice total in US: 110.00
Shipping cost in US: 11.00
Invoice total in CH: 110.00
Shipping cost in CH: 11.00
Annual tax in EU: 900.00

Calls to LegacyTaxCalculator.ComputeTax:
  ComputeAnnualTax, line 8: 1 call(s)
  ComputeShippingCost, line 11: 3 call(s)
  ComputeTotal, line 8: 3 call(s)
```

The call sites in `InvoiceService.ComputeRefund` and `TaxReport.ComputeQuarterlyTax` are not in the list, because they did not run. In a real application, you would run this build in a test or staging environment for some time, and write the counts to a log instead of the console.

To remove the counting, delete the fabric and the `CallCounter` class. The callers do not need any change.

> [!div class="see-also"]
> <xref:redirecting-calls-to-existing-methods>
> <xref:sample-legacy-replacement-2>
