namespace Metalama.Samples.LegacyTesting1;

public record Invoice( string Number, decimal Amount, int DaysOverdue );

// A static helper that queries the billing database. In a real application, each method opens a connection to the
// database server and runs a SQL query. In this example, the methods return constant data, so that the program can run.
public static class BillingDatabase
{
    public static string GetCustomerEmail( int customerId ) => $"customer{customerId}@example.com";

    public static IReadOnlyList<Invoice> GetOverdueInvoices( int customerId )
        => [new Invoice( "INV-1001", 120.00m, 12 ), new Invoice( "INV-1007", 45.50m, 41 )];
}
