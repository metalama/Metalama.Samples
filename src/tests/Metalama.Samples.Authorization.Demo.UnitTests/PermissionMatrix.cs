using Metalama.Samples.Authorization.Demo.Services;
using static Metalama.Samples.Authorization.Demo.UnitTests.Actor;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// The expected security model of the demo application: for each operation, the actors allowed to perform it.
/// <see cref="PermissionMatrixTests"/> runs every operation as every actor, so each row produces one test case
/// per actor. To cover a new operation, add a row.
/// </summary>
/// <remarks>
/// The rows describe the intended business rules, independently of how <see cref="DemoPolicy"/> implements them.
/// Each name starts with the identifier of the requirement in the README of the sample, for instance <c>INV-5</c>.
/// The sales invoice is owned by Sam, the marketing invoice by Mia and the engineering invoice by Tom.
/// </remarks>
public static class PermissionMatrix
{
    private static readonly Actor[] _everyone = [.. ActorExtensions.All];

    public static IReadOnlyList<Operation> Operations { get; } =
    [
        // Company directory.
        Op( "DIR-1: Read the company name", org => _ = org.Contoso.Name, _everyone ),
        Op( "DIR-1: Read a department name", org => _ = org.Sales.Name, _everyone ),
        Op( "DIR-1: Read the role assignments of a department", org => _ = org.Sales.RoleAssignments, _everyone ),
        Op( "DIR-2: Rename a department", org => org.Sales.Name = "Sales and Partnerships" ),

        // Reading invoices.
        Op( "INV-1: Read the number of an invoice", org => _ = org.SalesInvoice.Number, _everyone ),
        Op( "INV-2: Read a sales invoice", org => _ = org.SalesInvoice.Total, Sam, Mia, Ada, Otto, Grace ),
        Op( "INV-2: Read the marketing invoice", org => _ = org.MarketingInvoice.Total, Mia, Grace, Ada, Otto ),
        Op( "INV-2: Read an engineering invoice", org => _ = org.EngineeringInvoice.Total, Tom, Erin, Ada, Otto, Grace ),
        Op( "INV-2: Read the owner of a sales invoice", org => _ = org.SalesInvoice.Owner, Sam, Mia, Ada, Otto, Grace ),

        // Preparing invoices.
        Op( "INV-3: Create a sales invoice", org => org.Sales.CreateInvoice( org.Fabrikam ), Sam, Mia ),
        Op( "INV-3: Create a marketing invoice", org => org.Marketing.CreateInvoice( org.Fabrikam ), Mia ),
        Op( "INV-3: Create an engineering invoice", org => org.Engineering.CreateInvoice( org.Northwind ), Tom, Erin ),
        Op( "INV-3: Create an invoice for the customer of another department", org => org.Engineering.CreateInvoice( org.Fabrikam ) ),
        Op( "INV-4: Add a line to a sales invoice", org => org.SalesInvoice.AddLine( "Printing", 1, 60 ), Sam, Mia ),
        Op( "INV-4: Remove a line from a sales invoice", org => org.SalesInvoice.RemoveLine( 0 ), Sam, Mia ),
        Op( "INV-4: Change the due date of a sales invoice", org => org.SalesInvoice.DueDate = new DateOnly( 2026, 12, 31 ), Sam, Mia ),
        Op( "INV-4: Add a line to an engineering invoice", org => org.EngineeringInvoice.AddLine( "Testing", 1, 500 ), Tom, Erin ),

        // Approval: four-eyes principle and segregation of duties.
        Op( "INV-5: Approve a sales invoice", org => org.SalesInvoice.Approve(), Mia ),
        Op( "INV-5: Approve an engineering invoice", org => org.EngineeringInvoice.Approve(), Erin ),
        Op( "INV-5: Approve the marketing invoice, owned by its manager", org => org.MarketingInvoice.Approve() ),

        // Sending and collecting.
        Op(
            "INV-6: Send an approved sales invoice",
            org => org.SalesInvoice.SendAsync( org.Outbox, CancellationToken.None ),
            arrange: org => Approve( org ),
            Sam,
            Mia ),
        Op(
            "INV-7: Record a payment on a sent sales invoice",
            org =>
            {
                org.SalesInvoice.RecordPayment( 100 );

                return Task.CompletedTask;
            },
            arrange: async org =>
            {
                Approve( org );
                await org.SalesInvoice.SendAsync( org.Outbox, CancellationToken.None );
            },
            Ada ),
        Op( "INV-8: Void a sales invoice", org => org.SalesInvoice.Void( "Duplicate" ), Ada ),

        // Ownership and transfers.
        Op( "INV-9: Change the owner of a sales invoice", org => org.SalesInvoice.Owner = org.Mia, Mia ),
        Op( "INV-10: Move a sales invoice to Marketing", org => org.SalesInvoice.TransferTo( org.Marketing ), Mia ),
        Op( "INV-10: Move a sales invoice to Engineering", org => org.SalesInvoice.TransferTo( org.Engineering ) ),

        // Customers.
        Op( "CUS-1: Read a customer name", org => _ = org.Fabrikam.Name, Sam, Mia, Ada, Otto, Grace ),
        Op( "CUS-1: Read a customer credit limit", org => _ = org.Fabrikam.CreditLimit, Sam, Mia, Ada, Otto, Grace ),
        Op( "CUS-3: Read a customer tax ID", org => _ = org.Fabrikam.TaxId, Mia, Ada, Otto, Grace ),
        Op( "CUS-2: Change a customer email", org => org.Fabrikam.Email = "ap@fabrikam.example", Sam, Mia ),
        Op( "CUS-2: Change a customer tax ID", org => org.Fabrikam.TaxId = "CZ87654321", Sam, Mia ),
        Op( "CUS-4: Change a customer credit limit", org => org.Fabrikam.CreditLimit = 80_000, Mia ),
        Op( "CUS-4: Change customer payment terms", org => org.Fabrikam.PaymentTermDays = 60, Mia ),

        // Budgets.
        Op( "BUD-1: Read the sales budget", org => _ = org.SalesBudget.Remaining, Mia, Ada, Otto, Grace ),
        Op( "BUD-1: Read the engineering budget", org => _ = org.EngineeringBudget.Remaining, Erin, Ada, Otto, Grace ),
        Op( "BUD-2: Adjust the sales budget", org => org.SalesBudget.Adjust( 10_000 ), Ada ),
        Op( "BUD-2: Record spending on the engineering budget", org => org.EngineeringBudget.RecordSpending( 1_000 ), Ada ),

        // Role management.
        Op( "ACC-2: Assign a role in Sales", org => org.Sales.AssignRole( new User( "Nina" ), DemoRoles.DepartmentAssistant ), Alice, Mia ),
        Op( "ACC-2: Assign a role in Marketing", org => org.Marketing.AssignRole( new User( "Nina" ), DemoRoles.DepartmentAssistant ), Alice, Mia ),
        Op(
            "ACC-1: Create a user account",
            org => new StaffService( org.Db ).CreateUserAsync( "Nina", "Sales assistant" ),
            Alice ),
        Op( "ACC-1: Assign a company-wide role", org => org.Contoso.AssignRole( new User( "Nina" ), DemoRoles.Accountant ), Alice ),
        Op( "ACC-2: Revoke a role in Engineering", org => org.Engineering.RevokeRole( org.Tom, DemoRoles.DepartmentAssistant ), Alice, Erin )
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

    // C# prefers this overload over the Action overload for a lambda that returns a Task, so the task is awaited.
    private static Operation Op( string name, Func<DemoOrganization, Task> act, params Actor[] allowed )
        => new( name, allowed.ToHashSet(), act );

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
