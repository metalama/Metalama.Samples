using Metalama.Samples.Authorization.Demo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// The decisions shared by all requests (<see cref="CachingAuthorizationPolicy"/>) are invalidated when a change is
/// visible to the other requests, and a request that works on data loaded before a change cannot share its decisions.
/// </summary>
public class SharedCacheTests : IAsyncLifetime
{
    private const string _invoiceNumber = "MKT-0001";

    private DemoOrganization _org = null!;

    public async Task InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async Task DisposeAsync() => await this._org.DisposeAsync();

    private CachingAuthorizationPolicy CachingPolicy => this._org.RootServices.GetRequiredService<CachingAuthorizationPolicy>();

    private Task<decimal> ReadTotalAsync( string userName )
        => this._org.Requests.RunAsync( userName, async services => (await services.GetRequiredService<InvoiceService>().GetAsync( _invoiceNumber )).Total );

    [Fact]
    public async Task RequestWithOutdatedData_DoesNotShareItsDecisions()
    {
        // Mia gives the marketing invoice to Vera, who has no role: Vera reads it as its owner.
        await this._org.Requests.RunAsync( "Mia", services => services.GetRequiredService<InvoiceService>().ChangeOwnerAsync( _invoiceNumber, "Vera" ) );

        // A request of Vera loads the invoice.
        await using var scope = this._org.RootServices.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.InvoicingDbContext>();
        var invoice = await db.Invoices.SingleAsync( i => i.Number == _invoiceNumber );
        var vera = await db.Users.SingleAsync( u => u.Name == "Vera" );

        // Meanwhile, another request gives the invoice to Grace and saves the change.
        await this._org.Requests.RunAsync( "Mia", services => services.GetRequiredService<InvoiceService>().ChangeOwnerAsync( _invoiceNumber, "Grace" ) );

        // The first request still sees Vera as the owner in its own data.
        using ( SecurityContext.Use( new UserSecurityContext( vera, scope.ServiceProvider.GetRequiredService<IAuthorizationPolicy>() ) ) )
        {
            _ = invoice.Total;
        }

        // This decision was made from data loaded before the change, so it is not shared: a new request is denied.
        await Assert.ThrowsAsync<AccessDeniedException>( () => this.ReadTotalAsync( "Vera" ) );
    }

    [Fact]
    public async Task SaveInATransaction_InvalidatesAfterTheCommit()
    {
        await using var scope = this._org.RootServices.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.InvoicingDbContext>();
        var invoice = await db.Invoices.SingleAsync( i => i.Number == _invoiceNumber );
        var generation = this.CachingPolicy.Generation;

        await using ( var transaction = await db.Database.BeginTransactionAsync() )
        {
            using ( SecurityContext.RunAsSystem() )
            {
                invoice.DueDate = invoice.DueDate.AddDays( 10 );
            }

            await db.SaveChangesAsync();

            // The other requests cannot see the change before the commit, so the cache is not invalidated yet.
            Assert.Equal( generation, this.CachingPolicy.Generation );

            await transaction.CommitAsync();
        }

        Assert.True( this.CachingPolicy.Generation > generation );
    }

    [Fact]
    public async Task RolledBackTransaction_InvalidatesNothing()
    {
        await using var scope = this._org.RootServices.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.InvoicingDbContext>();
        var invoice = await db.Invoices.SingleAsync( i => i.Number == _invoiceNumber );
        var generation = this.CachingPolicy.Generation;

        await using ( var transaction = await db.Database.BeginTransactionAsync() )
        {
            using ( SecurityContext.RunAsSystem() )
            {
                invoice.DueDate = invoice.DueDate.AddDays( 10 );
            }

            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        Assert.Equal( generation, this.CachingPolicy.Generation );
    }
}
