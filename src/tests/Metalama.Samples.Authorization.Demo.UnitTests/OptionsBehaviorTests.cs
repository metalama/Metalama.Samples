using Metalama.Samples.Authorization.RoleBased;
using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// Run-time proof that the build-time <see cref="AuthorizationOptions"/> of the demo project took effect.
/// </summary>
public class OptionsBehaviorTests : IAsyncLifetime
{
    private DemoOrganization _org = null!;

    public async Task InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async Task DisposeAsync() => await this._org.DisposeAsync();

    private AccessDeniedException Denial( User user, Action action )
    {
        using ( this._org.SignIn( user ) )
        {
            return Assert.Throws<AccessDeniedException>( action );
        }
    }

    [Fact]
    public void FinanceNamespace_ReadingRequiresReadSensitive()
    {
        var exception = this.Denial( this._org.Sam, () => _ = this._org.SalesBudget.Amount );

        Assert.Equal( DemoPermissions.ReadSensitive, exception.Permission.Name );
        Assert.Equal( AccessKind.Read, exception.AccessKind );
    }

    [Fact]
    public void SalesNamespace_ReadingRequiresRead()
    {
        var exception = this.Denial( this._org.Tom, () => _ = this._org.SalesInvoice.Total );

        Assert.Equal( PermissionNames.Read, exception.Permission.Name );
    }

    [Fact]
    public void CustomerTypeOption_WritingRequiresApprove()
    {
        var exception = this.Denial( this._org.Sam, () => this._org.Fabrikam.CreditLimit = 1 );

        Assert.Equal( DemoPermissions.Approve, exception.Permission.Name );
        Assert.Equal( AccessKind.Write, exception.AccessKind );
    }

    [Fact]
    public void CustomerMemberOption_EmailRequiresOnlyWrite()
    {
        using ( this._org.SignIn( this._org.Sam ) )
        {
            this._org.Fabrikam.Email = "sam@contoso.example";
        }

        var exception = this.Denial( this._org.Ada, () => this._org.Fabrikam.Email = "ada@contoso.example" );
        Assert.Equal( PermissionNames.Write, exception.Permission.Name );
    }

    [Fact]
    public void ExplicitAttribute_WinsOverOptions()
    {
        var exception = this.Denial( this._org.Sam, () => _ = this._org.Fabrikam.TaxId );

        Assert.Equal( DemoPermissions.ReadSensitive, exception.Permission.Name );
    }

    [Fact]
    public void ProjectOption_UnannotatedMethodsRequireWrite()
    {
        var invoiceDenial = this.Denial( this._org.Ada, () => this._org.SalesInvoice.ChangeCustomer( this._org.Fabrikam ) );
        var budgetDenial = this.Denial( this._org.Mia, () => this._org.SalesBudget.Adjust( 1 ) );

        Assert.Equal( PermissionNames.Write, invoiceDenial.Permission.Name );
        Assert.Equal( AccessKind.Execute, invoiceDenial.AccessKind );
        Assert.Equal( PermissionNames.Write, budgetDenial.Permission.Name );
    }

    [Fact]
    public void ToString_IsNotSecured()
        => Assert.Equal( "invoice SAL-0001", this._org.SalesInvoice.ToString() );

    [Fact]
    public void DisabledMember_IsNotSecured_EvenWithoutContext()
        => Assert.Equal( "SAL-0001", this._org.SalesInvoice.Number );

    [Fact]
    public void SecurityParentOverrides_AreNotSecured_ThroughBaseMemberOption()
    {
        // The library excludes the abstract SecurityParent property; the option is inherited by the overrides
        // in the demo project, across the project boundary.
        IRoleBasedSecurable invoice = this._org.SalesInvoice;

        // The override reads Department, which is secured, so the exclusion does not disclose the parent to a user
        // who cannot read the invoice: the denial names Department, not SecurityParent.
        using ( this._org.SignIn( this._org.Vera ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => invoice.SecurityParent );
            Assert.Equal( "Invoice.Department", exception.Record.Member );
        }

        using ( this._org.SignIn( this._org.Sam ) )
        {
            Assert.Same( this._org.Sales, invoice.SecurityParent );
        }
    }

    [Fact]
    public void NonPublicProperties_AreSecured()
    {
        // LastInvoiceNumber is internal: properties are secured whatever their accessibility.
        var exception = Assert.Throws<InvalidOperationException>( () => _ = this._org.Sales.LastInvoiceNumber );
        Assert.Contains( "Department.LastInvoiceNumber", exception.Message, StringComparison.Ordinal );

        using ( this._org.SignIn( this._org.Tom ) )
        {
            Assert.Equal( 1, this._org.Sales.LastInvoiceNumber );
        }
    }
}
