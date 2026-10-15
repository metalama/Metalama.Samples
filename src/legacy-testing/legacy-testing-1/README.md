---
uid: sample-legacy-testing-1
level: 300
summary: "This example redirects the calls of legacy code to its hard-wired dependencies, such as Thread.Sleep, File.ReadAllText, a static database helper and SmtpClient.Send, to existing fake methods, only in the Test configuration, so that an xUnit project can test the code without changing it."
keywords: "legacy code, test seams, fakes, Thread.Sleep, File.ReadAllText, SmtpClient, Test configuration, RedirectToExistingMethod, project fabric, interceptors, Metalama.Extensions.Interceptors"
created-date: 2026-10-15
modified-date: 2026-10-15
---

# Legacy testing example, step 1: Replacing hard-wired dependencies with fakes

[!metalama-project-buttons .]

The legacy `InvoiceService` calls a static database helper, the file system, the SMTP server and the clock directly.
A unit test cannot run it: the test would need a database and an SMTP server, and it would wait one second for each
message.

In this step, interceptors redirect these calls to fake methods. The interceptors exist only in the `Test`
configuration, which is the build that the tests use. The source code of `InvoiceService` does not change. In the
`Test` build, the calls are rewritten as follows:

[!metalama-compare ../../tests/Metalama.Samples.LegacyTesting.Tests/RedirectToFakes.cs]

## The fakes

Each fake is a static class with a method that has the signature of the method that it replaces. Instead of calling
the dependency, the fake method reads or stores data in memory, so that a test can provide the inputs and check the
outputs. For instance, the fake of `SmtpClient.Send` stores the messages:

[!metalama-file TestSeams/FakeMailServer.cs]

The other fakes are `FakeBillingDatabase`, `FakeFileSystem` and `FakeClock`, in the `TestSeams` directory.
`FakeClock.Sleep` adds the duration to a counter instead of waiting, so that the tests run immediately.

## Redirecting the calls to the fakes

The following project fabric redirects the calls:

[!metalama-file TestSeams/TestSeamsFabric.cs]

* The fabric registers the interceptors only when the `TEST_SEAMS` preprocessor symbol is defined. The project
  defines this symbol only in the `Test` configuration. In the other configurations, the calls are not rewritten.
* Each `InterceptMethods` call selects the methods of one type, and `RedirectToExistingMethod` redirects their calls
  to a fake method. `Where` selects one overload: for instance, `Thread.Sleep( int )` but not
  `Thread.Sleep( TimeSpan )`.
* Metalama binds the parameters of the fake method by name. The first parameter of `FakeMailServer.Send` receives
  the `SmtpClient` on which the call is made. The other parameters have the names of the parameters of
  `SmtpClient.Send`, so they receive its arguments. See <xref:interceptor-binding-rules>.

The project file declares the `Test` configuration and defines the symbol:

```xml
<PropertyGroup>
    <Configurations>Debug;Release;Test</Configurations>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)'=='Test'">
    <DefineConstants>$(DefineConstants);TEST_SEAMS</DefineConstants>
</PropertyGroup>
```

## Testing InvoiceService

The xUnit project references the legacy project with the `SetConfiguration` metadata, so that it uses the `Test`
build of this project, whatever its own configuration:

```xml
<ProjectReference Include="..\..\legacy-testing\legacy-testing-1\Metalama.Samples.LegacyTesting1.csproj"
                  SetConfiguration="Configuration=Test"/>
```

The tests provide the data of the database and the template file to the fakes, call `InvoiceService`, and check the
messages, the summary and the time spent waiting:

[!metalama-file ../../tests/Metalama.Samples.LegacyTesting1.UnitTests/InvoiceServiceTests.cs]

The fakes store their state in static fields. Each test resets them, and the tests do not run in parallel.

## Limits

The tests check the logic of `InvoiceService`, such as the late fee and the content of the messages. They do not test
the binary that you release, and they do not test the real dependencies. Writing the fakes also requires you to know
what the dependencies return. When you do not know it, record the real interactions first, as described in
<xref:sample-legacy-testing-2>.

> [!div class="see-also"]
> <xref:sample-legacy-testing>
> <xref:redirecting-calls-to-existing-methods>
> <xref:interceptors-selecting-calls>
