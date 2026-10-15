using Metalama.Samples.LegacyTesting1;
using Metalama.Samples.LegacyTesting1.TestSeams;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// The fakes store their state in static fields, so the tests must not run in parallel.
[assembly: Parallelization( Mode = ParallelMode.None )]

namespace Metalama.Samples.LegacyTesting1.UnitTests;

public class InvoiceServiceTests
{
    private const int _customerId = 7;

    public InvoiceServiceTests()
    {
        FakeClock.Reset();
        FakeFileSystem.Reset();
        FakeBillingDatabase.Reset();
        FakeMailServer.Reset();

        FakeFileSystem.AddFile( "reminder-template.txt", "Invoice {Number}, {DaysOverdue} days: {Amount} EUR." );
        FakeBillingDatabase.CustomerEmails[_customerId] = "ana@example.com";
    }

    [Fact]
    public void SendsOneReminderPerOverdueInvoice()
    {
        FakeBillingDatabase.OverdueInvoices[_customerId] =
            [new Invoice( "INV-1", 100.00m, 5 ), new Invoice( "INV-2", 10.00m, 20 )];

        new InvoiceService().SendReminders( _customerId );

        Assert.Equal(
            [
                new SentMessage( "billing@example.com", "ana@example.com", "Payment reminder: invoice INV-1",
                    "Invoice INV-1, 5 days: 100.00 EUR." ),
                new SentMessage( "billing@example.com", "ana@example.com", "Payment reminder: invoice INV-2",
                    "Invoice INV-2, 20 days: 10.00 EUR." )
            ],
            FakeMailServer.SentMessages );
    }

    [Fact]
    public void AddsLateFeeAfter30Days()
    {
        FakeBillingDatabase.OverdueInvoices[_customerId] =
            [new Invoice( "INV-1", 100.00m, 30 ), new Invoice( "INV-2", 100.00m, 31 )];

        var summary = new InvoiceService().SendReminders( _customerId );

        Assert.Equal(
            "INV-1: 100.00 EUR sent to ana@example.com." + Environment.NewLine
                                                         + "INV-2: 125.00 EUR sent to ana@example.com." + Environment.NewLine,
            summary );
    }

    [Fact]
    public void WaitsOneSecondAfterEachMessage()
    {
        FakeBillingDatabase.OverdueInvoices[_customerId] =
            [new Invoice( "INV-1", 100.00m, 5 ), new Invoice( "INV-2", 10.00m, 20 )];

        new InvoiceService().SendReminders( _customerId );

        Assert.Equal( TimeSpan.FromSeconds( 2 ), FakeClock.Elapsed );
    }

    [Fact]
    public void SendsNothingWithoutOverdueInvoices()
    {
        var summary = new InvoiceService().SendReminders( _customerId );

        Assert.Empty( summary );
        Assert.Empty( FakeMailServer.SentMessages );
    }
}
