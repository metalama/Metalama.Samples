namespace Metalama.Samples.LegacyTesting1.TestSeams;

// Replaces the methods of BillingDatabase in the Test configuration. The tests add the data in memory.
public static class FakeBillingDatabase
{
    public static Dictionary<int, string> CustomerEmails { get; } = new();

    public static Dictionary<int, List<Invoice>> OverdueInvoices { get; } = new();

    public static string GetCustomerEmail( int customerId ) => CustomerEmails[customerId];

    public static IReadOnlyList<Invoice> GetOverdueInvoices( int customerId )
        => OverdueInvoices.TryGetValue( customerId, out var invoices ) ? invoices : [];

    public static void Reset()
    {
        CustomerEmails.Clear();
        OverdueInvoices.Clear();
    }
}
