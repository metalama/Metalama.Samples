using Metalama.Samples.Authorization;
using Metalama.Samples.Authorization.Demo;
using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.Demo.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder( args );
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole( options => options.SingleLine = true );
builder.Logging.AddFilter( "Microsoft", LogLevel.Warning );
builder.Services.AddInvoicingApplication();

using var host = builder.Build();
await DemoData.InitializeAsync( host.Services );

var requests = host.Services.GetRequiredService<RequestRunner>();

// Shortcuts to run a request as a user and call an application service.
Task InvoicesAsync( string user, Func<InvoiceService, Task> action ) => requests.RunAsync( user, services => action( services.GetRequiredService<InvoiceService>() ) );
Task CustomersAsync( string user, Func<CustomerService, Task> action ) => requests.RunAsync( user, services => action( services.GetRequiredService<CustomerService>() ) );
Task BudgetsAsync( string user, Func<BudgetService, Task> action ) => requests.RunAsync( user, services => action( services.GetRequiredService<BudgetService>() ) );
Task StaffAsync( string user, Func<StaffService, Task> action ) => requests.RunAsync( user, services => action( services.GetRequiredService<StaffService>() ) );

Console.WriteLine( "Outside of a request" );

await Expect.FailsAsync<InvalidOperationException>(
    "Loading an invoice succeeds, but reading its total without a security context fails",
    async () =>
    {
        await using var scope = host.Services.CreateAsyncScope();
        var invoice = await scope.ServiceProvider.GetRequiredService<InvoiceService>().GetAsync( "SAL-0001" );
        _ = invoice.Total;
    } );

Console.WriteLine( "Preparing an invoice" );
await Expect.AllowedAsync( "Sam adds a line to his own draft", () => InvoicesAsync( "Sam", s => s.AddLineAsync( "SAL-0001", "Report printing", 1, 60 ) ) );
await Expect.DeniedAsync( "Sam approves the invoice", () => InvoicesAsync( "Sam", s => s.ApproveAsync( "SAL-0001" ) ) );
await Expect.DeniedAsync( "Tom, from another department, reads the total", () => InvoicesAsync( "Tom", async s => _ = (await s.GetAsync( "SAL-0001" )).Total ) );
await Expect.AllowedAsync( "Tom reads the invoice number, which is not secured", () => InvoicesAsync( "Tom", async s => _ = (await s.GetAsync( "SAL-0001" )).Number ) );

Console.WriteLine( "Approving: the four-eyes principle and segregation of duties" );
await Expect.AllowedAsync( "Mia approves Sam's invoice", () => InvoicesAsync( "Mia", s => s.ApproveAsync( "SAL-0001" ) ) );
await Expect.FailsAsync<InvalidOperationException>( "Mia adds a line to the approved invoice (business rule)", () => InvoicesAsync( "Mia", s => s.AddLineAsync( "SAL-0001", "Extra", 1, 1 ) ) );
await Expect.DeniedAsync( "Mia approves her own marketing invoice", () => InvoicesAsync( "Mia", s => s.ApproveAsync( "MKT-0001" ) ) );
await Expect.DeniedAsync( "Grace, marketing manager but also auditor, approves the marketing invoice", () => InvoicesAsync( "Grace", s => s.ApproveAsync( "MKT-0001" ) ) );
await Expect.AllowedAsync( "Mia gives her marketing invoice to Grace", () => InvoicesAsync( "Mia", s => s.ChangeOwnerAsync( "MKT-0001", "Grace" ) ) );
await Expect.DeniedAsync( "Mia approves the invoice that she created, now owned by Grace", () => InvoicesAsync( "Mia", s => s.ApproveAsync( "MKT-0001" ) ) );

Console.WriteLine( "Sending and collecting" );
await Expect.AllowedAsync( "Sam sends the approved invoice", () => InvoicesAsync( "Sam", s => s.SendAsync( "SAL-0001" ) ) );
await Expect.DeniedAsync( "Sam records a payment", () => InvoicesAsync( "Sam", s => s.RecordPaymentAsync( "SAL-0001", 100 ) ) );
await Expect.AllowedAsync( "Ada records the full payment", () => InvoicesAsync( "Ada", async s => await s.RecordPaymentAsync( "SAL-0001", (await s.GetAsync( "SAL-0001" )).Total ) ) );
await Expect.AllowedAsync( "Ada voids the engineering invoice", () => InvoicesAsync( "Ada", s => s.VoidAsync( "ENG-0001", "Duplicate" ) ) );
await Expect.DeniedAsync( "Ada edits invoice content", () => InvoicesAsync( "Ada", s => s.AddLineAsync( "MKT-0001", "Discount", 1, -100 ) ) );
await Expect.DeniedAsync( "Otto voids the marketing invoice", () => InvoicesAsync( "Otto", s => s.VoidAsync( "MKT-0001", "Audit" ) ) );
await Expect.AllowedAsync( "Sam adds a note to the paid invoice", () => InvoicesAsync( "Sam", s => s.AddNoteAsync( "SAL-0001", "Customer asked for a copy." ) ) );
await Expect.DeniedAsync( "Ada adds a note to the invoice", () => InvoicesAsync( "Ada", s => s.AddNoteAsync( "SAL-0001", "Paid by bank transfer." ) ) );
await Expect.AllowedAsync( "Ada reads the notes", () => InvoicesAsync( "Ada", async s => _ = (await s.GetAsync( "SAL-0001" )).Notes.Count ) );

Console.WriteLine( "Customer data: per-type and per-member options" );
await Expect.AllowedAsync( "Sam updates the customer email", () => CustomersAsync( "Sam", s => s.ChangeEmailAsync( "Fabrikam", "invoices@fabrikam.example" ) ) );
await Expect.DeniedAsync( "Sam raises the credit limit", () => CustomersAsync( "Sam", s => s.ChangeCreditLimitAsync( "Fabrikam", 80_000 ) ) );
await Expect.DeniedAsync( "Sam reads the tax ID", () => CustomersAsync( "Sam", s => s.GetTaxIdAsync( "Fabrikam" ) ) );
await Expect.AllowedAsync( "Mia raises the credit limit", () => CustomersAsync( "Mia", s => s.ChangeCreditLimitAsync( "Fabrikam", 80_000 ) ) );
await Expect.AllowedAsync( "Mia reads the tax ID", () => CustomersAsync( "Mia", s => s.GetTaxIdAsync( "Fabrikam" ) ) );
await Expect.DeniedAsync( "Alice, administrator, reads the tax ID", () => CustomersAsync( "Alice", s => s.GetTaxIdAsync( "Fabrikam" ) ) );
await Expect.DeniedAsync( "Mia moves the customer to Engineering", () => CustomersAsync( "Mia", s => s.MoveToDepartmentAsync( "Fabrikam", "ENG" ) ) );

Console.WriteLine( "Budgets: namespace-level options" );
await Expect.DeniedAsync( "Sam reads the sales budget", () => BudgetsAsync( "Sam", s => s.GetRemainingAsync( "SAL", 2026 ) ) );
await Expect.AllowedAsync( "Sam reads the status of the sales budget, granted by the aggregate", () => BudgetsAsync( "Sam", s => s.GetStatusAsync( "SAL", 2026 ) ) );
await Expect.DeniedAsync( "Sam reads the status of the engineering budget", () => BudgetsAsync( "Sam", s => s.GetStatusAsync( "ENG", 2026 ) ) );
await Expect.AllowedAsync( "Mia reads her department's budget", () => BudgetsAsync( "Mia", s => s.GetRemainingAsync( "SAL", 2026 ) ) );
await Expect.DeniedAsync( "Mia reads the engineering budget", () => BudgetsAsync( "Mia", s => s.GetRemainingAsync( "ENG", 2026 ) ) );
await Expect.DeniedAsync( "Mia increases her budget", () => BudgetsAsync( "Mia", s => s.AdjustAsync( "SAL", 2026, 10_000 ) ) );
await Expect.AllowedAsync( "Ada increases the sales budget", () => BudgetsAsync( "Ada", s => s.AdjustAsync( "SAL", 2026, 10_000 ) ) );

Console.WriteLine( "Moving an invoice: checking the parameter" );
await Expect.AllowedAsync( "Mia creates a sales invoice", () => InvoicesAsync( "Mia", s => s.CreateAsync( "SAL", "Fabrikam" ) ) );
await Expect.AllowedAsync( "Mia moves it to Marketing, which she also manages", () => InvoicesAsync( "Mia", s => s.TransferAsync( "SAL-0002", "MKT" ) ) );
await Expect.DeniedAsync( "Mia moves it to Engineering", () => InvoicesAsync( "Mia", s => s.TransferAsync( "SAL-0002", "ENG" ) ) );
await Expect.DeniedAsync( "Sam bills an invoice to a customer that he cannot see", () => InvoicesAsync( "Sam", s => s.ChangeCustomerAsync( "SAL-0001", "Northwind Traders" ) ) );
await Expect.DeniedAsync( "Tom creates an invoice for a customer of another department", () => InvoicesAsync( "Tom", s => s.CreateAsync( "ENG", "Fabrikam" ) ) );

Console.WriteLine( "Managing staff" );
await Expect.DeniedAsync( "Mia creates a user account", () => StaffAsync( "Mia", s => s.CreateUserAsync( "Nina", "Sales assistant" ) ) );
await Expect.AllowedAsync( "Alice creates a user account for Nina", () => StaffAsync( "Alice", s => s.CreateUserAsync( "Nina", "Sales assistant" ) ) );
await Expect.DeniedAsync( "Sam makes Nina a sales assistant", () => StaffAsync( "Sam", s => s.AssignDepartmentRoleAsync( "SAL", "Nina", DemoRoles.DepartmentAssistant ) ) );
await Expect.AllowedAsync( "Mia makes Nina a sales assistant", () => StaffAsync( "Mia", s => s.AssignDepartmentRoleAsync( "SAL", "Nina", DemoRoles.DepartmentAssistant ) ) );
await Expect.DeniedAsync( "Mia makes Nina an engineering assistant", () => StaffAsync( "Mia", s => s.AssignDepartmentRoleAsync( "ENG", "Nina", DemoRoles.DepartmentAssistant ) ) );
await Expect.DeniedAsync( "Mia makes herself an accountant of Sales", () => StaffAsync( "Mia", s => s.AssignDepartmentRoleAsync( "SAL", "Mia", DemoRoles.Accountant ) ) );
await Expect.DeniedAsync( "Alice makes herself an accountant", () => StaffAsync( "Alice", s => s.AssignCompanyRoleAsync( "Alice", DemoRoles.Accountant ) ) );
await Expect.AllowedAsync( "Nina reads the sales invoice", () => InvoicesAsync( "Nina", async s => _ = (await s.GetAsync( "SAL-0001" )).Total ) );
await Expect.AllowedAsync( "Alice revokes Nina's role", () => StaffAsync( "Alice", s => s.RevokeDepartmentRoleAsync( "SAL", "Nina", DemoRoles.DepartmentAssistant ) ) );
await Expect.DeniedAsync( "Nina reads the sales invoice", () => InvoicesAsync( "Nina", async s => _ = (await s.GetAsync( "SAL-0001" )).Total ) );

Console.WriteLine();
Console.WriteLine( "Outbox:" );

foreach ( var message in host.Services.GetRequiredService<OutboxInvoiceSender>().Messages )
{
    Console.WriteLine( $"  {message.Number} to {message.To}: {message.Total:N2}" );
}

Console.WriteLine();
Console.WriteLine( Expect.Failures == 0 ? "All scenarios behaved as expected." : $"{Expect.Failures} scenario(s) did NOT behave as expected." );

return Expect.Failures == 0 ? 0 : 1;

internal static class Expect
{
    public static int Failures { get; private set; }

    public static async Task AllowedAsync( string description, Func<Task> action )
    {
        try
        {
            await action();
            Report( true, description, "succeeded" );
        }
        catch ( AccessDeniedException e )
        {
            Report( false, description, $"was denied: {e.Message}" );
        }
    }

    public static async Task DeniedAsync( string description, Func<Task> action )
    {
        try
        {
            await action();
            Report( false, description, "succeeded but should have been denied" );
        }
        catch ( AccessDeniedException e )
        {
            Report( true, description, $"was denied: {e.Message}" );
        }
    }

    public static async Task FailsAsync<TException>( string description, Func<Task> action )
        where TException : Exception
    {
        try
        {
            await action();
            Report( false, description, $"succeeded but should have thrown {typeof(TException).Name}" );
        }
        catch ( TException e )
        {
            Report( true, description, $"failed: {e.Message}" );
        }
    }

    private static void Report( bool expected, string description, string outcome )
    {
        if ( !expected )
        {
            Failures++;
        }

        Console.WriteLine( $"  {(expected ? "GOOD" : "BAD ")} {description}: {outcome}" );
    }
}
