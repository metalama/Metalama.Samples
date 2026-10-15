using Metalama.Samples.Authorization.Demo.Finance;
using Metalama.Samples.Authorization.Demo.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Metalama.Samples.Authorization.Demo.DemoRoles;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// Creates the database schema and the sample data: the rules of the policy, Contoso and its departments, people,
/// customers, invoices and budgets.
/// </summary>
public static class DemoData
{
    public static async Task InitializeAsync( IServiceProvider services, CancellationToken cancellationToken = default )
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var policy = scope.ServiceProvider.GetRequiredService<DatabaseAuthorizationPolicy>();

        await db.Database.EnsureCreatedAsync( cancellationToken );

        // The rules come first: the policy is needed to create the invoices.
        db.PermissionRules.AddRange( DemoPolicy.CreateSeedRules() );
        await db.SaveChangesAsync( cancellationToken );
        await policy.ReloadAsync( db, cancellationToken );

        // Seeding is a trusted operation, like a database migration.
        using ( SecurityContext.RunAsSystem() )
        {
            Seed( db, policy );
        }

        await db.SaveChangesAsync( cancellationToken );
    }

    private static void Seed( InvoicingDbContext db, IAuthorizationPolicy policy )
    {
        var alice = new User( "Alice", "Administrator" );
        var ada = new User( "Ada", "Accountant" );
        var otto = new User( "Otto", "Auditor" );
        var grace = new User( "Grace", "Auditor and marketing manager" );
        var mia = new User( "Mia", "Sales and marketing manager" );
        var sam = new User( "Sam", "Sales assistant" );
        var erin = new User( "Erin", "Engineering manager" );
        var tom = new User( "Tom", "Engineering assistant" );
        var vera = new User( "Vera", "Visitor without any role" );

        db.Users.AddRange( alice, ada, otto, grace, mia, sam, erin, tom, vera );

        var contoso = new Company( "Contoso" );
        var sales = new Department( contoso, "SAL", "Sales" );
        var marketing = new Department( contoso, "MKT", "Marketing" );
        var engineering = new Department( contoso, "ENG", "Engineering" );

        // Company-wide roles.
        contoso.AssignRole( alice, Administrator );
        contoso.AssignRole( ada, Accountant );
        contoso.AssignRole( otto, Auditor );
        contoso.AssignRole( grace, Auditor );

        // Department roles.
        sales.AssignRole( mia, DepartmentManager );
        sales.AssignRole( sam, DepartmentAssistant );
        marketing.AssignRole( mia, DepartmentManager );
        marketing.AssignRole( grace, DepartmentManager );
        engineering.AssignRole( erin, DepartmentManager );
        engineering.AssignRole( tom, DepartmentAssistant );

        db.Companies.Add( contoso );
        db.Departments.AddRange( sales, marketing, engineering );

        var fabrikam = new Customer( sales, "Fabrikam" ) { Email = "billing@fabrikam.example", TaxId = "CZ12345678", CreditLimit = 50_000 };
        var northwind = new Customer( engineering, "Northwind Traders" ) { CreditLimit = 20_000 };
        db.Customers.AddRange( fabrikam, northwind );

        // Each invoice is created by its department's staff, who becomes its owner.
        var salesInvoice = CreateInvoice( sales, fabrikam, sam, policy );
        salesInvoice.Lines.Add( new InvoiceLine( "Consulting, March", 10, 1_200 ) );
        salesInvoice.Lines.Add( new InvoiceLine( "Travel expenses", 1, 340 ) );

        var marketingInvoice = CreateInvoice( marketing, fabrikam, mia, policy );
        marketingInvoice.Lines.Add( new InvoiceLine( "Trade fair booth", 1, 4_500 ) );

        var engineeringInvoice = CreateInvoice( engineering, northwind, tom, policy );
        engineeringInvoice.Lines.Add( new InvoiceLine( "Prototype", 1, 8_000 ) );

        db.Invoices.AddRange( salesInvoice, marketingInvoice, engineeringInvoice );

        db.Budgets.AddRange( new Budget( sales, 2026, 250_000 ), new Budget( engineering, 2026, 400_000 ) );
    }

    private static Invoice CreateInvoice( Department department, Customer customer, User creator, IAuthorizationPolicy policy )
    {
        // CreateInvoice makes the current subject the owner.
        using ( SecurityContext.Use( new UserSecurityContext( creator, policy ) ) )
        {
            return department.CreateInvoice( customer );
        }
    }
}
