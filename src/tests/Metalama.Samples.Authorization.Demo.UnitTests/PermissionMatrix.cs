using static Metalama.Samples.Authorization.Demo.UnitTests.Actor;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// The expected security model of the demo application: for each operation, the actors allowed to perform it.
/// <see cref="PermissionMatrixTests"/> runs every operation as every actor, so each row produces one test case
/// per actor. To cover a new operation, add a row.
/// </summary>
/// <remarks>
/// The rows describe the intended business rules, independently of how <see cref="DemoPolicy"/> implements them.
/// The sales invoice is owned by Sam, the marketing invoice by Mia and the engineering invoice by Tom.
/// </remarks>
public static class PermissionMatrix
{
    private static readonly Actor[] _everyone = [.. ActorExtensions.All];

    public static IReadOnlyList<Operation> Operations { get; } =
    [
        // Company directory.
        Op( "Read the company name", org => _ = org.Contoso.Name, _everyone ),
        Op( "Read a department name", org => _ = org.Sales.Name, _everyone ),
        Op( "Read the role assignments of a department", org => _ = org.Sales.RoleAssignments, _everyone ),
        Op( "Rename a department", org => org.Sales.Name = "Sales and Partnerships" ),

        // Reading invoices.
        Op( "Read the number of an invoice", org => _ = org.SalesInvoice.Number, _everyone ),
        Op( "Read a sales invoice", org => _ = org.SalesInvoice.Total, Sam, Mia, Ada, Otto, Grace ),
        Op( "Read the marketing invoice", org => _ = org.MarketingInvoice.Total, Mia, Grace, Ada, Otto ),
        Op( "Read an engineering invoice", org => _ = org.EngineeringInvoice.Total, Tom, Erin, Ada, Otto, Grace ),
        Op( "Read the owner of a sales invoice", org => _ = org.SalesInvoice.Owner, Sam, Mia, Ada, Otto, Grace ),

        // Preparing invoices.
        Op( "Create a sales invoice", org => org.Sales.CreateInvoice( org.Fabrikam ), Sam, Mia ),
        Op( "Create a marketing invoice", org => org.Marketing.CreateInvoice( org.Fabrikam ), Mia ),
        Op( "Create an engineering invoice", org => org.Engineering.CreateInvoice( org.Northwind ), Tom, Erin ),
        Op( "Create an invoice for the customer of another department", org => org.Engineering.CreateInvoice( org.Fabrikam ) ),
        Op( "Add a line to a sales invoice", org => org.SalesInvoice.AddLine( "Printing", 1, 60 ), Sam, Mia ),
        Op( "Remove a line from a sales invoice", org => org.SalesInvoice.RemoveLine( 0 ), Sam, Mia ),
        Op( "Change the due date of a sales invoice", org => org.SalesInvoice.DueDate = new DateOnly( 2026, 12, 31 ), Sam, Mia ),
        Op( "Add a line to an engineering invoice", org => org.EngineeringInvoice.AddLine( "Testing", 1, 500 ), Tom, Erin ),

        // Approval: four-eyes principle and segregation of duties.
        Op( "Approve a sales invoice", org => org.SalesInvoice.Approve(), Mia ),
        Op( "Approve an engineering invoice", org => org.EngineeringInvoice.Approve(), Erin ),
        Op( "Approve the marketing invoice, owned by its manager", org => org.MarketingInvoice.Approve() ),

        // Sending and collecting.
        Op(
            "Send an approved sales invoice",
            org => org.SalesInvoice.SendAsync( CancellationToken.None ),
            arrange: org => Approve( org ),
            Sam,
            Mia ),
        Op(
            "Record a payment on a sent sales invoice",
            org =>
            {
                org.SalesInvoice.RecordPayment( 100 );

                return Task.CompletedTask;
            },
            arrange: async org =>
            {
                Approve( org );
                await org.SalesInvoice.SendAsync( CancellationToken.None );
            },
            Ada ),
        Op( "Void a sales invoice", org => org.SalesInvoice.Void( "Duplicate" ), Ada ),

        // Ownership and transfers.
        Op( "Change the owner of a sales invoice", org => org.SalesInvoice.Owner = org.Mia, Mia ),
        Op( "Move a sales invoice to Marketing", org => org.SalesInvoice.TransferTo( org.Marketing ), Mia ),
        Op( "Move a sales invoice to Engineering", org => org.SalesInvoice.TransferTo( org.Engineering ) ),

        // Customers.
        Op( "Read a customer name", org => _ = org.Fabrikam.Name, Sam, Mia, Ada, Otto, Grace ),
        Op( "Read a customer credit limit", org => _ = org.Fabrikam.CreditLimit, Sam, Mia, Ada, Otto, Grace ),
        Op( "Read a customer tax ID", org => _ = org.Fabrikam.TaxId, Mia, Ada, Otto, Grace ),
        Op( "Change a customer email", org => org.Fabrikam.Email = "ap@fabrikam.example", Sam, Mia ),
        Op( "Change a customer tax ID", org => org.Fabrikam.TaxId = "CZ87654321", Sam, Mia ),
        Op( "Change a customer credit limit", org => org.Fabrikam.CreditLimit = 80_000, Mia ),
        Op( "Change customer payment terms", org => org.Fabrikam.PaymentTermDays = 60, Mia ),

        // Budgets.
        Op( "Read the sales budget", org => _ = org.SalesBudget.Remaining, Mia, Ada, Otto, Grace ),
        Op( "Read the engineering budget", org => _ = org.EngineeringBudget.Remaining, Erin, Ada, Otto, Grace ),
        Op( "Adjust the sales budget", org => org.SalesBudget.Adjust( 10_000 ), Ada ),
        Op( "Record spending on the engineering budget", org => org.EngineeringBudget.RecordSpending( 1_000 ), Ada ),

        // Role management.
        Op( "Assign a role in Sales", org => org.Sales.AssignRole( new User( "Nina" ), DemoRoles.DepartmentAssistant ), Alice, Mia ),
        Op( "Assign a role in Marketing", org => org.Marketing.AssignRole( new User( "Nina" ), DemoRoles.DepartmentAssistant ), Alice, Mia ),
        Op( "Assign a company-wide role", org => org.Contoso.AssignRole( new User( "Nina" ), DemoRoles.Accountant ), Alice ),
        Op( "Revoke a role in Engineering", org => org.Engineering.RevokeRole( org.Tom, DemoRoles.DepartmentAssistant ), Alice, Erin )
    ];

    public static Operation Get( string name ) => Operations.Single( o => o.Name == name );

    private static void Approve( DemoOrganization org ) => org.SalesInvoice.Approve();

    private static Operation Op( string name, Action<DemoOrganization> act, params Actor[] allowed )
        => new(
            name,
            allowed.ToHashSet(),
            org =>
            {
                act( org );

                return Task.CompletedTask;
            } );

    private static Operation Op(
        string name,
        Func<DemoOrganization, Task> act,
        Action<DemoOrganization> arrange,
        params Actor[] allowed )
        => new(
            name,
            allowed.ToHashSet(),
            act,
            org =>
            {
                arrange( org );

                return Task.CompletedTask;
            } );

    private static Operation Op(
        string name,
        Func<DemoOrganization, Task> act,
        Func<DemoOrganization, Task> arrange,
        params Actor[] allowed )
        => new( name, allowed.ToHashSet(), act, arrange );
}
