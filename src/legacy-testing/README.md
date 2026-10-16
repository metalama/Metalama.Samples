---
uid: sample-legacy-testing
summary: "How to get legacy code under test without changing it: Metalama interceptors create a seam at each call to a hard-wired dependency, so that the tests can use fakes, or replay the interactions recorded while the application runs."
keywords: "legacy code, seams, characterization tests, golden master, snapshot testing, record and replay, fakes, test seams, interceptors, Metalama.Extensions.Interceptors"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Example: getting legacy code under test without refactoring

Michael Feathers, in _Working Effectively with Legacy Code_ (Prentice Hall, 2004), defines legacy code as code
without tests. Such code is risky to change, because nothing detects a regression. However, to add tests, you
usually need to change the code first, for instance to replace a call to the database with a call to an interface
that a test can implement. This series shows how to get such code under test without editing it.

The legacy code of this series is an `InvoiceService` class. It sends a payment reminder for each overdue invoice of
a customer. It calls its dependencies directly:

* `BillingDatabase.GetCustomerEmail` and `BillingDatabase.GetOverdueInvoices`, the methods of a static helper that
  queries the database,
* `File.ReadAllText`, to read the template of the message,
* `SmtpClient.Send`, to send the message,
* `Thread.Sleep`, to respect the rate limit of the SMTP server.

[!metalama-file legacy-testing-1/InvoiceService.cs]

A test of this code would need a database and an SMTP server, and it would wait one second for each message.

## Seams

Feathers defines a _seam_ as a place where you can alter the behavior of a program without editing it in that place.
Each seam has an _enabling point_: the place where you decide which behavior to use. In object-oriented code, the most
common seam is a virtual method or an interface, and the enabling point is the code that creates the object. Code
that calls static methods directly, such as `InvoiceService`, has no such seam.

A Metalama interceptor is a seam at each call site. The build rewrites the calls to the selected methods so that they
call another method, and the source code of the calls does not change. The enabling point is a fabric, which
registers the interceptors only in some build configurations. For the comparable seams of other languages, see the
_link seams_ and _preprocessing seams_ of Feathers. Martin Fowler describes the same idea in the article
[Legacy Seam](https://martinfowler.com/bliki/LegacySeam.html).

With these seams, you can apply a safe workflow to legacy code:

1. Get the code under test without changing it, with fakes or with recorded interactions.
2. Pin its current behavior with _characterization tests_. Feathers defines a characterization test as a test that
   describes the actual behavior of the code, and not the behavior that the code should have. This technique is also
   known as _golden master testing_ or _approval testing_. In .NET, libraries such as
   [Verify](https://github.com/VerifyTests/Verify) and [ApprovalTests](https://github.com/approvals/ApprovalTests.Net)
   implement it.
3. Refactor the code toward real abstractions, while the tests detect regressions.
4. Remove the interceptors.

## In this series

| Article | Description |
|---------|-------------|
| <xref:sample-legacy-testing-1> | In the `Test` configuration, interceptors redirect the calls to existing fake methods. An xUnit project tests `InvoiceService`. |
| <xref:sample-legacy-testing-2> | A T# template records each call, with its arguments and its result, to a JSON file while the application runs. |
| <xref:sample-legacy-testing-3> | A T# template replays the recorded results, and a snapshot test pins the output of `InvoiceService`. |

## Limits of the approach

* The tests do not test the binary that you release. In the `Test` configuration, the build rewrites the calls to the
  dependencies, and the release build does not. The tests prove the behavior of the code around these calls, but
  not of the calls themselves. Keep the difference small: intercept only the calls to the dependencies that the tests
  cannot use.
* Metalama intercepts only the calls in the source code of the current project. It does not intercept the calls in
  other assemblies, for instance the calls that a library makes internally, or in the output of source generators.
* Interceptors intercept only the uses of methods. They do not intercept property accesses such as `DateTime.Now`,
  constructors such as `new SmtpClient()`, operators, or `await` expressions. In this example, the constructor of
  `SmtpClient` and the assignments of its properties still run in the tests. This is acceptable because they do not
  connect to the server. Code that reads `DateTime.Now` needs another seam, for instance a `TimeProvider`, before you
  can test it.
* The fakes and the replayer store their state in static fields, so the tests cannot run in parallel.
* A recording contains the data of the environment in which you made it. When you record in production, the
  recording can contain personal data. Review or anonymize it before you commit it.
* A replay test checks the order of the calls and their arguments. A refactoring that changes the order of the calls
  breaks the test, even when the behavior of the program does not change. Use replay tests to pin the behavior
  before the refactoring, and replace them with ordinary unit tests afterward.
* Metalama supports only SDK-style projects. An old-style project file must be converted to the SDK style before you
  can use interceptors. This conversion changes the project file, not the code. Projects that target .NET Framework
  are supported. See <xref:requirements>.
* The `meta.Interception` property, which the templates of steps 2 and 3 use, is a C# 14 extension property. In a
  project that uses an earlier version of C#, use `meta.GetExtension<IInterceptionInfo>()` instead.
* The `Metalama.Extensions.Interceptors` package requires a Metalama Community license or higher.

## Next step: refactoring and removing the interceptors

Interceptors are a temporary step. When the tests pin the behavior of `InvoiceService`, refactor it toward real
abstractions: inject an `IBillingRepository`, a file provider, an `IEmailSender` and a `TimeProvider` instead of
calling the static methods. The tests then use test doubles of these abstractions, and they test the binary that
you release. When no code calls the intercepted methods anymore, delete the fabric and the fakes. The calls are no
longer rewritten, and the `Test` configuration is no longer needed.

> [!div class="see-also"]
> <xref:interceptors>
> <xref:interceptors-use-cases>
> <xref:redirecting-calls-to-existing-methods>
> <xref:generating-interceptors-with-templates>
