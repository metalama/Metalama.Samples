---
uid: sample-legacy-testing-3
level: 300
summary: "This example replays the recorded interactions of legacy code with its dependencies in a Test configuration, with a T# template interceptor, and pins the output of the code with a snapshot test, also known as a characterization test or golden master test."
keywords: "legacy code, record and replay, characterization tests, golden master, snapshot testing, approval testing, T# template, meta.Cast, interceptors, Metalama.Extensions.Interceptors"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Legacy testing example, step 3: Replaying interactions in characterization tests

[!metalama-project-buttons .]

Before you refactor `InvoiceService`, you want a test that detects any change of its behavior. Feathers calls such a
test a _characterization test_: it describes what the code actually does, not what it should do. A common form of
characterization test is the _snapshot test_, also known as _golden master test_ or _approval test_: the test
compares the output of the code with a file that was approved earlier.

In this step, the `Test` configuration replays the interactions that the `Record` configuration recorded, as
described in <xref:sample-legacy-testing-2>. A snapshot test then pins the output of `InvoiceService`.

## Recording and replaying

The project has two configurations:

* In the `Record` configuration, the interceptors record the calls to `interactions.json`, as in the previous step.
* In the `Test` configuration, the interceptors do not call the dependencies. They return the recorded results.

The following project fabric chooses the template from the preprocessor symbols that each configuration defines:

[!metalama-file Recording/RecordAndReplayFabric.cs]

In the `Replay` template, `meta.Proceed()` is not called, so the dependency is never called. For a method that
returns a value, the template gives the return type of the intercepted method to the replayer as a `Type`, with
`ToType()`, and casts the result to this type with `meta.Cast`. For a method that returns `void`, it only gives the
call to the replayer. Metalama generates the following interceptors:

[!metalama-compare ../../tests/Metalama.Samples.LegacyTesting.Tests/ReplayInteractions.cs]

The replayer takes the recorded interactions in order. For each call, it checks that the code calls the recorded
method with the recorded arguments, and it returns the recorded result. A difference means that the behavior of the
code has changed, so the replayer throws an exception:

[!metalama-file Recording/InteractionReplayer.cs]

## The characterization test

To create the test, run the program in the `Record` configuration, and copy `interactions.json` to the `Recordings`
directory of the test project. The test loads the recording, calls `InvoiceService`, and checks that all the recorded
calls were made. It then compares the summary that `InvoiceService` returns with a snapshot file:

[!metalama-file ../../tests/Metalama.Samples.LegacyTesting3.UnitTests/InvoiceServiceCharacterizationTests.cs]

The test checks the following:

* Each call to the dependencies, including the content of each message that the code sends, because the replayer
  compares the arguments with the recording.
* The order of the calls, including the calls to `Thread.Sleep`.
* The value that `SendReminders` returns, because the snapshot file contains it:

```
INV-1001: 120.00 EUR sent to customer42@example.com.
INV-1007: 70.50 EUR sent to customer42@example.com.
```

The snapshot helper is a minimal implementation of snapshot testing. When the value differs from the
`.verified.txt` file, it writes the value to a `.received.txt` file and fails. To accept a new behavior, replace the
`.verified.txt` file with the `.received.txt` file. Libraries such as [Verify](https://github.com/VerifyTests/Verify)
provide the same workflow with diff tools.

[!metalama-file ../../tests/Metalama.Samples.LegacyTesting3.UnitTests/Snapshot.cs]

For instance, when you change the late fee from 25.00 to 30.00 EUR, the test fails, because the second message
differs from the recording:

```
System.InvalidOperationException : The code calls SmtpClient.Send with the argument "Dear customer,\n\nInvoice INV-1007 is 41 days overdue. The amount due is 75.50 EUR.\n\nBilling department\n", but the recording has "Dear customer,\n\nInvoice INV-1007 is 41 days overdue. The amount due is 70.50 EUR.\n\nBilling department\n".
```

## Next step

The characterization test pins the behavior of `InvoiceService`, so you can now refactor it. Replace the calls to
the static methods with calls to injected abstractions, and replace the characterization test with unit tests that
use test doubles of these abstractions. Then delete the fabric, the recorder and the replayer. See the last section
of <xref:sample-legacy-testing>.

> [!div class="see-also"]
> <xref:sample-legacy-testing>
> <xref:generating-interceptors-with-templates>
> <xref:interceptors-use-cases>
