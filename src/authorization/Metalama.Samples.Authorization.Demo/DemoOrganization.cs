using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.Demo.Finance;
using Metalama.Samples.Authorization.Demo.Sales;
using Metalama.Samples.Authorization.Demo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// The sample organization loaded from the database, with direct access to its objects. It gives tests and
/// examples a short way to reach the domain objects, which application code reaches through the services.
/// </summary>
/// <remarks>
/// The objects are loaded through a database context of a dependency injection scope, without any security
/// context: Entity Framework does not go through the secured accessors.
/// </remarks>
public sealed class DemoOrganization : IAsyncDisposable
{
    private readonly ServiceProvider? _ownedProvider;
    private readonly AsyncServiceScope _scope;

    private DemoOrganization( IServiceProvider rootServices, ServiceProvider? ownedProvider )
    {
        this.RootServices = rootServices;
        this._ownedProvider = ownedProvider;
        this._scope = rootServices.CreateAsyncScope();
    }

    /// <summary>
    /// Gets the root service provider of the application.
    /// </summary>
    public IServiceProvider RootServices { get; }

    /// <summary>
    /// Gets the service provider of the scope in which the objects are loaded.
    /// </summary>
    public IServiceProvider Services => this._scope.ServiceProvider;

    public InvoicingDbContext Db => this.Services.GetRequiredService<InvoicingDbContext>();

    public IAuthorizationPolicy Policy => this.Services.GetRequiredService<IAuthorizationPolicy>();

    public RequestRunner Requests => this.Services.GetRequiredService<RequestRunner>();

    public OutboxInvoiceSender Outbox => this.Services.GetRequiredService<OutboxInvoiceSender>();

    // People.
    public User Alice { get; private set; } = null!;

    public User Ada { get; private set; } = null!;

    public User Otto { get; private set; } = null!;

    public User Grace { get; private set; } = null!;

    public User Mia { get; private set; } = null!;

    public User Sam { get; private set; } = null!;

    public User Erin { get; private set; } = null!;

    public User Tom { get; private set; } = null!;

    public User Vera { get; private set; } = null!;

    // Organization.
    public Company Contoso { get; private set; } = null!;

    public Department Sales { get; private set; } = null!;

    public Department Marketing { get; private set; } = null!;

    public Department Engineering { get; private set; } = null!;

    // Business data.
    public Customer Fabrikam { get; private set; } = null!;

    public Customer Northwind { get; private set; } = null!;

    public Invoice SalesInvoice { get; private set; } = null!;

    public Invoice MarketingInvoice { get; private set; } = null!;

    public Invoice EngineeringInvoice { get; private set; } = null!;

    public Budget SalesBudget { get; private set; } = null!;

    public Budget EngineeringBudget { get; private set; } = null!;

    /// <summary>
    /// Creates an application with a new in-memory database, seeds it, and loads the organization.
    /// </summary>
    public static async Task<DemoOrganization> CreateAsync(
        Action<IServiceCollection>? configureServices = null,
        CancellationToken cancellationToken = default )
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInvoicingApplication();
        configureServices?.Invoke( services );

        var provider = services.BuildServiceProvider( new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true } );
        await DemoData.InitializeAsync( provider, cancellationToken );

        var org = new DemoOrganization( provider, provider );
        await org.LoadAsync( cancellationToken );

        return org;
    }

    /// <summary>
    /// Loads the organization again in a new scope, with a new database context, from what is saved in the database.
    /// </summary>
    public async Task<DemoOrganization> ReloadAsync( CancellationToken cancellationToken = default )
    {
        var org = new DemoOrganization( this.RootServices, null );
        await org.LoadAsync( cancellationToken );

        return org;
    }

    /// <summary>
    /// Makes <paramref name="user"/> the current subject until the returned object is disposed.
    /// </summary>
    public IDisposable SignIn( User user, IAccessAuditor? auditor = null )
        => SecurityContext.Use( new UserSecurityContext( user, this.Policy, auditor ) );

    public Task SaveChangesAsync( CancellationToken cancellationToken = default ) => this.Db.SaveChangesAsync( cancellationToken );

    public async ValueTask DisposeAsync()
    {
        await this._scope.DisposeAsync();

        if ( this._ownedProvider != null )
        {
            await this._ownedProvider.DisposeAsync();
        }
    }

    private async Task LoadAsync( CancellationToken cancellationToken )
    {
        var db = this.Db;

        var users = await db.Users.ToDictionaryAsync( u => u.Name, cancellationToken );
        this.Alice = users["Alice"];
        this.Ada = users["Ada"];
        this.Otto = users["Otto"];
        this.Grace = users["Grace"];
        this.Mia = users["Mia"];
        this.Sam = users["Sam"];
        this.Erin = users["Erin"];
        this.Tom = users["Tom"];
        this.Vera = users["Vera"];

        this.Contoso = await db.Companies.SingleAsync( cancellationToken );
        this.Sales = await db.Departments.SingleAsync( d => d.Code == "SAL", cancellationToken );
        this.Marketing = await db.Departments.SingleAsync( d => d.Code == "MKT", cancellationToken );
        this.Engineering = await db.Departments.SingleAsync( d => d.Code == "ENG", cancellationToken );

        this.Fabrikam = await db.Customers.SingleAsync( c => c.Name == "Fabrikam", cancellationToken );
        this.Northwind = await db.Customers.SingleAsync( c => c.Name == "Northwind Traders", cancellationToken );

        this.SalesInvoice = await db.Invoices.SingleAsync( i => i.Number == "SAL-0001", cancellationToken );
        this.MarketingInvoice = await db.Invoices.SingleAsync( i => i.Number == "MKT-0001", cancellationToken );
        this.EngineeringInvoice = await db.Invoices.SingleAsync( i => i.Number == "ENG-0001", cancellationToken );

        this.SalesBudget = await db.Budgets.SingleAsync( b => b.Department.Code == "SAL", cancellationToken );
        this.EngineeringBudget = await db.Budgets.SingleAsync( b => b.Department.Code == "ENG", cancellationToken );
    }
}
