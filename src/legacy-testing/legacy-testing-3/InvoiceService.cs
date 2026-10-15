using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace Metalama.Samples.LegacyTesting3;

// Legacy code. It calls the database, the file system, the clock and the mail server directly.
public class InvoiceService
{
    private const decimal _lateFee = 25.00m;

    // Sends a payment reminder for each overdue invoice of a customer, and returns a summary of the reminders.
    public string SendReminders( int customerId )
    {
        var email = BillingDatabase.GetCustomerEmail( customerId );
        var invoices = BillingDatabase.GetOverdueInvoices( customerId );
        var template = File.ReadAllText( "reminder-template.txt" );

        using var smtp = new SmtpClient();

        // In production, the messages go to an SMTP server. In this example, they go to the outbox directory.
        smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
        smtp.PickupDirectoryLocation = Path.GetFullPath( "outbox" );

        var summary = new StringBuilder();

        foreach ( var invoice in invoices )
        {
            // A late fee applies after 30 days.
            var amountDue = invoice.DaysOverdue > 30 ? invoice.Amount + _lateFee : invoice.Amount;
            var formattedAmount = amountDue.ToString( "F2", CultureInfo.InvariantCulture );
            var daysOverdue = invoice.DaysOverdue.ToString( CultureInfo.InvariantCulture );

            var body = template
                .Replace( "{Number}", invoice.Number, StringComparison.Ordinal )
                .Replace( "{DaysOverdue}", daysOverdue, StringComparison.Ordinal )
                .Replace( "{Amount}", formattedAmount, StringComparison.Ordinal );

            smtp.Send( "billing@example.com", email, $"Payment reminder: invoice {invoice.Number}", body );

            summary.AppendLine(
                CultureInfo.InvariantCulture,
                $"{invoice.Number}: {formattedAmount} EUR sent to {email}." );

            // The SMTP server accepts at most one message per second.
            Thread.Sleep( 1000 );
        }

        return summary.ToString();
    }
}
