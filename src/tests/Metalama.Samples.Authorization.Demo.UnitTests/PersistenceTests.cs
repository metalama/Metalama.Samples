using Metalama.Samples.Authorization.Demo.Sales;
using Metalama.Samples.Authorization.Demo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// Tests of the interaction between the authorization aspects and Entity Framework, and of the request
/// architecture of the application.
/// </summary>
public class PersistenceTests : IAsyncLifetime
{
    private DemoOrganization _org = null!;

    public async ValueTask InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async ValueTask DisposeAsync() => await this._org.DisposeAsync();

    [Fact]
    public async Task Loading_DoesNotCallSecuredAccessors()
    {
        Assert.Null( SecurityContext.Current );

        // Materializing every object, with its navigations and role assignments, runs no check.
        await using var reloaded = await this._org.ReloadAsync();

        Assert.Equal( "SAL-0001", reloaded.SalesInvoice.Number );
        Assert.Throws<InvalidOperationException>( () => reloaded.SalesInvoice.Total );
    }

    [Fact]
    public async Task Saving_DoesNotCallSecuredAccessors()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            this._org.SalesInvoice.Lines.Add( new InvoiceLine( "Printing", 1, 60 ) );
            this._org.Fabrikam.CreditLimit = 60_000;
        }

        // Detecting the changes and saving them, outside of any security context, runs no check.
        await this._org.SaveChangesAsync();
    }

    [Fact]
    public void Model_MapsEveryFieldOfEveryEntity()
    {
        // A field that is not mapped is state that is silently lost when the object is saved. Entity Framework maps
        // only properties with a setter by convention, so get-only properties such as Invoice.Status must be mapped
        // explicitly. The fields that Metalama generates for overridden auto-properties are included.
        var unmapped = new List<string>();

        foreach ( var entityType in this._org.Db.Model.GetEntityTypes() )
        {
            var mappedFields = entityType.GetProperties()
                .Select( p => p.FieldInfo )
                .Concat( entityType.GetNavigations().Select( n => n.FieldInfo ) )
                .OfType<FieldInfo>()
                .ToHashSet();

            for ( var type = entityType.ClrType; type != null && type != typeof(object) && type.Assembly == typeof(Entity).Assembly; type = type.BaseType )
            {
                foreach ( var field in type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly ) )
                {
                    if ( !mappedFields.Contains( field ) )
                    {
                        unmapped.Add( $"{entityType.DisplayName()}: {field.DeclaringType!.Name}.{field.Name}" );
                    }
                }
            }
        }

        Assert.Empty( unmapped );
    }

    [Fact]
    public async Task AuthorizedChange_IsPersisted()
    {
        using ( this._org.SignIn( this._org.Mia ) )
        {
            this._org.SalesInvoice.Approve();
            await this._org.SaveChangesAsync();
        }

        await using var reloaded = await this._org.ReloadAsync();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( InvoiceStatus.Approved, reloaded.SalesInvoice.Status );
        }
    }

    [Fact]
    public async Task DeniedChange_LeavesTheDatabaseUnchanged()
    {
        using ( this._org.SignIn( this._org.Sam ) )
        {
            Assert.Throws<AccessDeniedException>( () => this._org.Fabrikam.CreditLimit = 1_000_000 );
            await this._org.SaveChangesAsync();
        }

        await using var reloaded = await this._org.ReloadAsync();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( 50_000m, reloaded.Fabrikam.CreditLimit );
        }
    }

    [Fact]
    public async Task RoleAssignment_IsPersisted_AndEffectiveInANewContext()
    {
        using ( this._org.SignIn( this._org.Mia ) )
        {
            this._org.Sales.AssignRole( new User( "Nina" ), DemoRoles.DepartmentAssistant );
            await this._org.SaveChangesAsync();
        }

        await using var reloaded = await this._org.ReloadAsync();
        var nina = await reloaded.Db.Users.SingleAsync( u => u.Name == "Nina" );

        using ( reloaded.SignIn( nina ) )
        {
            Assert.Equal( 12_340m, reloaded.SalesInvoice.Total );
        }
    }

    [Fact]
    public async Task Queries_AreNotFilteredByAuthorization()
    {
        // The aspects protect members, not rows: a user without any role can query invoices, but cannot read them.
        // Filtering rows requires another mechanism, for instance an Entity Framework global query filter.
        using ( this._org.SignIn( this._org.Vera ) )
        {
            var invoices = await this._org.Db.Invoices.Where( i => i.Customer.Name == "Fabrikam" ).ToListAsync();

            Assert.Equal( 2, invoices.Count );
            Assert.Throws<AccessDeniedException>( () => invoices[0].Total );
        }
    }

    [Fact]
    public async Task InvoiceLifecycle_ThroughServices_EachStepInItsOwnRequest()
    {
        var requests = this._org.Requests;
        string number = null!;

        await requests.RunAsync(
            "Sam",
            async services =>
            {
                var invoices = services.GetRequiredService<InvoiceService>();
                number = (await invoices.CreateAsync( "SAL", "Fabrikam" )).Number;
                await invoices.AddLineAsync( number, "Workshop", 2, 750 );
            } );

        await requests.RunAsync( "Mia", services => services.GetRequiredService<InvoiceService>().ApproveAsync( number ) );
        await requests.RunAsync( "Sam", services => services.GetRequiredService<InvoiceService>().SendAsync( number ) );
        await requests.RunAsync( "Ada", services => services.GetRequiredService<InvoiceService>().RecordPaymentAsync( number, 1_500 ) );

        var status = await requests.RunAsync( "Otto", async services => (await services.GetRequiredService<InvoiceService>().GetAsync( number )).Status );

        Assert.Equal( InvoiceStatus.Paid, status );
        Assert.Contains( this._org.Outbox.Messages, m => m.Number == number && m.To == "billing@fabrikam.example" && m.Total == 1_500 );
    }

    [Fact]
    public async Task DeniedRequest_ThroughServices_SavesNothing()
    {
        var requests = this._org.Requests;

        await Assert.ThrowsAsync<AccessDeniedException>(
            () => requests.RunAsync( "Sam", services => services.GetRequiredService<CustomerService>().ChangeCreditLimitAsync( "Fabrikam", 1_000_000 ) ) );

        var creditLimit = await requests.RunAsync( "Mia", async services => (await services.GetRequiredService<CustomerService>().GetAsync( "Fabrikam" )).CreditLimit );

        Assert.Equal( 50_000m, creditLimit );
    }

    [Fact]
    public async Task ConcurrentRequests_HaveTheirOwnSecurityContext()
    {
        var requests = this._org.Requests;
        var bothStarted = new Barrier( 2 );

        Task<bool> CanReadSalesInvoiceAsync( string user )
            => requests.RunAsync(
                user,
                async services =>
                {
                    var invoice = await services.GetRequiredService<InvoiceService>().GetAsync( "SAL-0001" );

                    // Both requests are in progress at this point, each with its own context and connection.
                    bothStarted.SignalAndWait( TimeSpan.FromSeconds( 10 ) );

                    try
                    {
                        _ = invoice.Total;

                        return true;
                    }
                    catch ( AccessDeniedException )
                    {
                        return false;
                    }
                } );

        var results = await Task.WhenAll( Task.Run( () => CanReadSalesInvoiceAsync( "Sam" ) ), Task.Run( () => CanReadSalesInvoiceAsync( "Tom" ) ) );

        Assert.True( results[0] );
        Assert.False( results[1] );
    }

    [Fact]
    public async Task UnknownUser_CannotStartARequest()
        => await Assert.ThrowsAsync<InvalidOperationException>( () => this._org.Requests.RunAsync( "Mallory", _ => Task.CompletedTask ) );

    [Fact]
    public async Task NotesAddedThroughTheSecuredView_ArePersisted()
    {
        using ( this._org.SignIn( this._org.Sam ) )
        {
            this._org.SalesInvoice.Notes.Add( "Call back on Monday" );
            await this._org.SaveChangesAsync();
        }

        await using var reloaded = await this._org.ReloadAsync();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( ["Call back on Monday"], reloaded.SalesInvoice.Notes );
        }
    }

    [Fact]
    public async Task Materialization_CreatesEntitiesInASystemScope_AndKeepsAllValues()
    {
        // The interceptor creates each entity with its parameterless constructor, in a system scope, so constructors
        // can set secured properties. Entity Framework then sets every value through the backing fields.
        await using var reloaded = await this._org.ReloadAsync();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( "Contoso", reloaded.Contoso.Name );
            Assert.Equal( ("SAL", "Sales"), (reloaded.Sales.Code, reloaded.Sales.Name) );
            Assert.Equal( "Fabrikam", reloaded.Fabrikam.Name );
            Assert.Equal( 2026, reloaded.SalesBudget.FiscalYear );
            Assert.Equal( 2, reloaded.SalesInvoice.Lines.Count );
            Assert.IsType<SecurableList<InvoiceLine>>( reloaded.SalesInvoice.Lines );
            Assert.IsType<SecurableList<string>>( reloaded.SalesInvoice.Notes );
        }
    }

    [Fact]
    public async Task LinesAddedThroughTheSecurableList_ArePersisted()
    {
        using ( this._org.SignIn( this._org.Sam ) )
        {
            this._org.SalesInvoice.Lines.Add( new InvoiceLine( "Printing", 1, 60 ) );
            await this._org.SaveChangesAsync();
        }

        await using var reloaded = await this._org.ReloadAsync();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( 3, reloaded.SalesInvoice.Lines.Count );
            Assert.Equal( 12_400m, reloaded.SalesInvoice.Total );
        }
    }
}
