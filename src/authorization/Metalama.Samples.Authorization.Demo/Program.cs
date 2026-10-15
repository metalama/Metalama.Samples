using Metalama.Samples.Authorization;
using Metalama.Samples.Authorization.Demo;
using Metalama.Samples.Authorization.Demo.Sales;

var org = DemoOrganization.Create();
var auditor = new ConsoleAuditor();
var sales = org.SalesInvoice;

Console.WriteLine( "No security context" );
Expect.Fails<InvalidOperationException>( "Reading an invoice total without signing in", () => _ = sales.Total );

Console.WriteLine( "Preparing an invoice" );

using ( org.SignIn( org.Sam, auditor ) )
{
    Expect.Allowed( "Sam adds a line to his own draft", () => sales.AddLine( "Report printing", 1, 60 ) );
    Expect.Allowed( "Sam changes the due date", () => sales.DueDate = sales.DueDate.AddDays( 15 ) );
    Expect.Denied( "Sam approves the invoice", sales.Approve );
}

using ( org.SignIn( org.Tom, auditor ) )
{
    Expect.Denied( "Tom, from another department, reads the total", () => _ = sales.Total );
    Expect.Allowed( "Tom reads the invoice number, which is not secured", () => _ = sales.Number );
}

Console.WriteLine( "Approving: the four-eyes principle and segregation of duties" );

using ( org.SignIn( org.Mia, auditor ) )
{
    Expect.Allowed( "Mia approves Sam's invoice", sales.Approve );
    Expect.Fails<InvalidOperationException>( "Mia adds a line to the approved invoice (business rule)", () => sales.AddLine( "Extra", 1, 1 ) );
    Expect.Denied( "Mia approves her own marketing invoice", org.MarketingInvoice.Approve );
}

using ( org.SignIn( org.Grace, auditor ) )
{
    Expect.Denied( "Grace, marketing manager but also auditor, approves the marketing invoice", org.MarketingInvoice.Approve );
}

Console.WriteLine( "Sending and collecting" );

using ( org.SignIn( org.Sam, auditor ) )
{
    await Expect.AllowedAsync( "Sam sends the approved invoice", () => sales.SendAsync( CancellationToken.None ) );
    Expect.Denied( "Sam records a payment", () => sales.RecordPayment( 100 ) );
}

using ( org.SignIn( org.Ada, auditor ) )
{
    Expect.Allowed( "Ada records the full payment", () => sales.RecordPayment( sales.Total ) );
    Expect.Allowed( "Ada voids the engineering invoice", () => org.EngineeringInvoice.Void( "Duplicate" ) );
    Expect.Denied( "Ada edits invoice content", () => org.MarketingInvoice.AddLine( "Discount", 1, -100 ) );
}

using ( org.SignIn( org.Otto, auditor ) )
{
    Expect.Allowed( "Otto reads the paid amount", () => _ = sales.AmountPaid );
    Expect.Denied( "Otto voids the marketing invoice", () => org.MarketingInvoice.Void( "Audit" ) );
}

Console.WriteLine( "Customer data: per-type and per-member options" );

using ( org.SignIn( org.Sam, auditor ) )
{
    Expect.Allowed( "Sam updates the customer email", () => org.Fabrikam.Email = "invoices@fabrikam.example" );
    Expect.Denied( "Sam raises the credit limit", () => org.Fabrikam.CreditLimit = 80_000 );
    Expect.Denied( "Sam reads the tax ID", () => _ = org.Fabrikam.TaxId );
}

using ( org.SignIn( org.Mia, auditor ) )
{
    Expect.Allowed( "Mia raises the credit limit", () => org.Fabrikam.CreditLimit = 80_000 );
    Expect.Allowed( "Mia reads the tax ID", () => _ = org.Fabrikam.TaxId );
}

using ( org.SignIn( org.Alice, auditor ) )
{
    Expect.Denied( "Alice, administrator, reads the tax ID", () => _ = org.Fabrikam.TaxId );
}

Console.WriteLine( "Budgets: namespace-level options" );

using ( org.SignIn( org.Sam, auditor ) )
{
    Expect.Denied( "Sam reads the sales budget", () => _ = org.SalesBudget.Remaining );
}

using ( org.SignIn( org.Mia, auditor ) )
{
    Expect.Allowed( "Mia reads her department's budget", () => _ = org.SalesBudget.Remaining );
    Expect.Denied( "Mia reads the engineering budget", () => _ = org.EngineeringBudget.Remaining );
    Expect.Denied( "Mia increases her budget", () => org.SalesBudget.Adjust( 10_000 ) );
}

using ( org.SignIn( org.Ada, auditor ) )
{
    Expect.Allowed( "Ada increases the sales budget", () => org.SalesBudget.Adjust( 10_000 ) );
}

Console.WriteLine( "Moving an invoice: checking the parameter" );

using ( org.SignIn( org.Mia, auditor ) )
{
    var draft = org.Sales.CreateInvoice( org.Fabrikam );
    Expect.Allowed( "Mia moves a draft from Sales to Marketing, which she also manages", () => draft.TransferTo( org.Marketing ) );
    Expect.Denied( "Mia moves the draft to Engineering", () => draft.TransferTo( org.Engineering ) );
}

using ( org.SignIn( org.Tom, auditor ) )
{
    Expect.Denied( "Tom creates an invoice for a customer of another department", () => org.Engineering.CreateInvoice( org.Fabrikam ) );
}

Console.WriteLine( "Managing roles" );

var nina = new User( "Nina (new sales assistant)" );

using ( org.SignIn( org.Sam, auditor ) )
{
    Expect.Denied( "Sam makes Nina an assistant", () => org.Sales.AssignRole( nina, DemoRoles.DepartmentAssistant ) );
}

using ( org.SignIn( org.Mia, auditor ) )
{
    Expect.Allowed( "Mia makes Nina a sales assistant", () => org.Sales.AssignRole( nina, DemoRoles.DepartmentAssistant ) );
    Expect.Denied( "Mia makes Nina an engineering assistant", () => org.Engineering.AssignRole( nina, DemoRoles.DepartmentAssistant ) );
}

using ( org.SignIn( nina, auditor ) )
{
    Expect.Allowed( "Nina reads the sales invoice", () => _ = sales.Total );
}

using ( org.SignIn( org.Alice, auditor ) )
{
    Expect.Allowed( "Alice revokes Nina's role", () => org.Sales.RevokeRole( nina, DemoRoles.DepartmentAssistant ) );
}

using ( org.SignIn( nina, auditor ) )
{
    Expect.Denied( "Nina reads the sales invoice", () => _ = sales.Total );
}

Console.WriteLine();
Console.WriteLine( Expect.Failures == 0 ? "All scenarios behaved as expected." : $"{Expect.Failures} scenario(s) did NOT behave as expected." );

return Expect.Failures == 0 ? 0 : 1;

internal static class Expect
{
    public static int Failures { get; private set; }

    public static void Allowed( string description, Action action )
    {
        try
        {
            action();
            Report( true, description, "succeeded" );
        }
        catch ( AccessDeniedException e )
        {
            Report( false, description, $"was denied: {e.Message}" );
        }
    }

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

    public static void Denied( string description, Action action )
    {
        try
        {
            action();
            Report( false, description, "succeeded but should have been denied" );
        }
        catch ( AccessDeniedException e )
        {
            Report( true, description, $"was denied: {e.Message}" );
        }
    }

    public static void Fails<TException>( string description, Action action )
        where TException : Exception
    {
        try
        {
            action();
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
