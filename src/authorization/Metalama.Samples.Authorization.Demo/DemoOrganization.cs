using Metalama.Samples.Authorization.Demo.Finance;
using Metalama.Samples.Authorization.Demo.Sales;
using Metalama.Samples.Authorization.RoleBased;
using static Metalama.Samples.Authorization.Demo.DemoRoles;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// The sample organization: Contoso and its departments, people, customers, invoices and budgets.
/// It is shared by the console program and the unit tests.
/// </summary>
public sealed class DemoOrganization
{
    private DemoOrganization()
    {
        this.Contoso = new Company( "Contoso" );
        this.Sales = new Department( this.Contoso, "SAL", "Sales" );
        this.Marketing = new Department( this.Contoso, "MKT", "Marketing" );
        this.Engineering = new Department( this.Contoso, "ENG", "Engineering" );

        // Company-wide roles.
        this.Contoso.AssignRole( this.Alice, Administrator );
        this.Contoso.AssignRole( this.Ada, Accountant );
        this.Contoso.AssignRole( this.Otto, Auditor );
        this.Contoso.AssignRole( this.Grace, Auditor );

        // Department roles.
        this.Sales.AssignRole( this.Mia, DepartmentManager );
        this.Sales.AssignRole( this.Sam, DepartmentAssistant );
        this.Marketing.AssignRole( this.Mia, DepartmentManager );
        this.Marketing.AssignRole( this.Grace, DepartmentManager );
        this.Engineering.AssignRole( this.Erin, DepartmentManager );
        this.Engineering.AssignRole( this.Tom, DepartmentAssistant );

        this.Fabrikam = new Customer( this.Sales, "Fabrikam" )
        {
            Email = "billing@fabrikam.example", TaxId = "CZ12345678", CreditLimit = 50_000
        };

        this.Northwind = new Customer( this.Engineering, "Northwind Traders" ) { CreditLimit = 20_000 };

        // Each invoice is created by its department's staff, who becomes its owner.
        this.SalesInvoice = this.CreateInvoice( this.Sales, this.Fabrikam, this.Sam );
        this.SalesInvoice.AddLine( "Consulting, March", 10, 1_200 );
        this.SalesInvoice.AddLine( "Travel expenses", 1, 340 );

        this.MarketingInvoice = this.CreateInvoice( this.Marketing, this.Fabrikam, this.Mia );
        this.MarketingInvoice.AddLine( "Trade fair booth", 1, 4_500 );

        this.EngineeringInvoice = this.CreateInvoice( this.Engineering, this.Northwind, this.Tom );
        this.EngineeringInvoice.AddLine( "Prototype", 1, 8_000 );

        this.SalesBudget = new Budget( this.Sales, 2026, 250_000 );
        this.EngineeringBudget = new Budget( this.Engineering, 2026, 400_000 );
    }

    public RoleBasedAuthorizationPolicy Policy { get; } = DemoPolicy.Create();

    // People.
    public User Alice { get; } = new( "Alice (administrator)" );

    public User Ada { get; } = new( "Ada (accountant)" );

    public User Otto { get; } = new( "Otto (auditor)" );

    public User Grace { get; } = new( "Grace (auditor, marketing manager)" );

    public User Mia { get; } = new( "Mia (sales and marketing manager)" );

    public User Sam { get; } = new( "Sam (sales assistant)" );

    public User Erin { get; } = new( "Erin (engineering manager)" );

    public User Tom { get; } = new( "Tom (engineering assistant)" );

    public User Vera { get; } = new( "Vera (no role)" );

    // Organization.
    public Company Contoso { get; }

    public Department Sales { get; }

    public Department Marketing { get; }

    public Department Engineering { get; }

    // Business data.
    public Customer Fabrikam { get; }

    public Customer Northwind { get; }

    public Invoice SalesInvoice { get; }

    public Invoice MarketingInvoice { get; }

    public Invoice EngineeringInvoice { get; }

    public Budget SalesBudget { get; }

    public Budget EngineeringBudget { get; }

    /// <summary>
    /// Creates the organization. The data is loaded in a system scope, as it would be from a database.
    /// </summary>
    public static DemoOrganization Create()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            return new DemoOrganization();
        }
    }

    /// <summary>
    /// Makes <paramref name="user"/> the current subject until the returned object is disposed.
    /// </summary>
    public IDisposable SignIn( User user, IAccessAuditor? auditor = null )
        => SecurityContext.Use( new UserSecurityContext( user, this.Policy, auditor ) );

    private Invoice CreateInvoice( Department department, Customer customer, User creator )
    {
        using ( SecurityContext.Use( new UserSecurityContext( creator, this.Policy ) ) )
        {
            return department.CreateInvoice( customer );
        }
    }
}
