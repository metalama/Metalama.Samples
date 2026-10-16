---
uid: sample-legacy-testing-2
level: 300
summary: "This example records each call of legacy code to its dependencies, with its arguments and its result, to a JSON file while the application runs, with a T# template interceptor enabled only in a Record configuration."
keywords: "legacy code, record and replay, recording interactions, JSON, characterization tests, T# template, RedirectToSynthesizedMethod, meta.Proceed, interceptors, Metalama.Extensions.Interceptors"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Legacy testing example, step 2: Recording real interactions

[!metalama-project-buttons .]

To write the fakes of the previous step, you must know what the dependencies return. With legacy code, this knowledge
is often lost: the database contains data that nobody fully understands, and the template file has changed since the
code was written. Instead of guessing, you can record what the code does when it runs in a real environment.

In this step, a T# template records each call to the dependencies of `InvoiceService`: the called method, the
arguments, and the result. When the program exits, the recorder writes the calls to `interactions.json`. The
recording exists only in the `Record` configuration.

Record and replay is a common technique for tests that depend on HTTP services: for instance, the
[VCR](https://github.com/vcr/vcr) library for Ruby records HTTP interactions to a file and replays them. With
interceptors, you can record the calls to any method of the source code, not only HTTP requests.

## Running the program with the recording

Build and run the program in the `Record` configuration:

```
dotnet run -c Record
```

The program writes the same summary as without the recording. The calls to `Thread.Sleep` and to the SMTP client are
real. The recorder writes the following file:

```json
[
  {
    "Method": "BillingDatabase.GetCustomerEmail",
    "Arguments": [ 42 ],
    "Result": "customer42@example.com"
  },
  {
    "Method": "BillingDatabase.GetOverdueInvoices",
    "Arguments": [ 42 ],
    "Result": [
      { "Number": "INV-1001", "Amount": 120.00, "DaysOverdue": 12 },
      { "Number": "INV-1007", "Amount": 45.50, "DaysOverdue": 41 }
    ]
  },
  {
    "Method": "File.ReadAllText",
    "Arguments": [ "reminder-template.txt" ],
    "Result": "Dear customer,\n\nInvoice {Number} is {DaysOverdue} days overdue. The amount due is {Amount} EUR.\n\nBilling department\n"
  },
  {
    "Method": "SmtpClient.Send",
    "Arguments": [
      "billing@example.com",
      "customer42@example.com",
      "Payment reminder: invoice INV-1001",
      "Dear customer,\n\nInvoice INV-1001 is 12 days overdue. The amount due is 120.00 EUR.\n\nBilling department\n"
    ],
    "Result": null
  },
  {
    "Method": "Thread.Sleep",
    "Arguments": [ 1000 ],
    "Result": null
  }
]
```

The file continues with the second message and the second call to `Thread.Sleep`.

## Implementation

The following project fabric registers the interceptors when the `RECORD_INTERACTIONS` symbol is defined. The project
defines this symbol only in the `Record` configuration.

[!metalama-file Recording/RecordingFabric.cs]

* `RedirectToSynthesizedMethod( nameof(this.Record) )` generates one interceptor for each intercepted method from the
  `Record` template. Unlike the fake methods of the previous step, one template handles all the signatures.
* The template reads the arguments of the call from `meta.Interception.Method.ArgumentParameters`. It calls the real
  method with `meta.Proceed()`, and gives the method name, the arguments and the result to the recorder.
* The name of the method is computed when the template is expanded, so the generated code contains it as a constant.

Metalama generates the following interceptors, in the `MetalamaInterceptors` class. For a method that returns `void`,
the result is `null`.

[!metalama-compare ../../tests/Metalama.Samples.LegacyTesting.Tests/RecordInteractions.cs]

The recorder is ordinary run-time code. It serializes the arguments and the results with `System.Text.Json`, and it
writes the file when the process exits:

[!metalama-file Recording/InteractionRecorder.cs]

## Limits

* The arguments and the results must be serializable to JSON. The recorder does not record the object on which an
  instance method is called, such as the `SmtpClient`.
* The recorder does not record exceptions. To record them, add a `try`/`catch` block around `meta.Proceed()` to the
  template.
* The recording contains the data of the environment. Review or anonymize it before you commit it.

The next step uses the recording to test `InvoiceService`. See <xref:sample-legacy-testing-3>.

> [!div class="see-also"]
> <xref:sample-legacy-testing>
> <xref:generating-interceptors-with-templates>
> <xref:interceptors-selecting-calls>
