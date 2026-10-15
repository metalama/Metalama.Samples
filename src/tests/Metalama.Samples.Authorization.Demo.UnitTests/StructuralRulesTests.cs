using Metalama.Samples.Authorization.Demo.Sales;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// Rules that the code guarantees by its structure, independently of the policy: they hold even if a rule
/// such as <c>Allow&lt;object&gt;( Write, ... )</c> is added later.
/// </summary>
public class StructuralRulesTests : IAsyncLifetime
{
    private DemoOrganization _org = null!;

    public async Task InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async Task DisposeAsync() => await this._org.DisposeAsync();

    [Theory]
    [InlineData( typeof(Company), nameof(Company.Name) )]
    [InlineData( typeof(Department), nameof(Department.Name) )]
    public void DIR2_CompanyAndDepartmentNames_AreInitOnly( Type type, string propertyName )
    {
        // DIR-2: nobody can rename the company or a department in the application.
        var setter = type.GetProperty( propertyName )!.SetMethod!;

        Assert.Contains( typeof(IsExternalInit), setter.ReturnParameter.GetRequiredCustomModifiers() );
    }

    [Theory]
    [InlineData( typeof(Invoice), nameof(Invoice.Customer) )]
    [InlineData( typeof(Customer), nameof(Customer.Department) )]
    public void ReferencesToOtherSecuredObjects_ChangeOnlyThroughCheckedMethods( Type type, string propertyName )
    {
        // INV-4 and CUS-5: a setter could not check the new value, so it is private, and a method with a parameter
        // check (Invoice.ChangeCustomer, Customer.MoveTo) changes the reference.
        var setter = type.GetProperty( propertyName )!.SetMethod;

        Assert.True( setter is null || setter.IsPrivate );
    }

    [Fact]
    public void InvoiceLines_AreTheSecurableListOfTheInvoice()
    {
        using ( this._org.SignIn( this._org.Ada ) )
        {
            var lines = this._org.SalesInvoice.Lines;

            // The property returns the list itself, the same instance each time, owned by the invoice.
            Assert.Same( lines, this._org.SalesInvoice.Lines );
            Assert.Same( this._org.SalesInvoice, Assert.IsType<SecurableList<InvoiceLine>>( lines ).Owner );

            // Ada can read the lines, but changing them requires Write.
            Assert.Throws<AccessDeniedException>( () => lines.Add( new InvoiceLine( "Gift", 1, 1_000 ) ) );
        }
    }

    [Fact]
    public void Creator_CannotChange_AfterTheOwnerChanges()
    {
        using ( this._org.SignIn( this._org.Mia ) )
        {
            this._org.MarketingInvoice.Owner = this._org.Grace;

            Assert.Equal( this._org.Mia, this._org.MarketingInvoice.Creator );
            Assert.Null( typeof(Invoice).GetProperty( nameof(Invoice.Creator) )!.SetMethod );
        }
    }
}
