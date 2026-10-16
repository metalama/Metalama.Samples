using Metalama.Samples.Authorization.Demo.Sales;
using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// Multi-step business workflows. Single permissions are covered by <see cref="PermissionMatrix"/>; these tests
/// cover what a single operation cannot show: state changes, the effect of role changes over time, and the
/// interaction between authorization and business rules.
/// </summary>
public class WorkflowTests : IAsyncLifetime
{
    private DemoOrganization _org = null!;

    public async Task InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async Task DisposeAsync() => await this._org.DisposeAsync();

    private void As( User user, Action action )
    {
        using ( this._org.SignIn( user ) )
        {
            action();
        }
    }

    private void Denied( User user, Action action ) => this.As( user, () => Assert.Throws<AccessDeniedException>( action ) );

    // Runs a request that changes rights and saves them, like a request of the application. The cache of decisions
    // shared by all requests is invalidated when the change is saved.
    private void AsAndSave( User user, Action action )
    {
        this.As( user, action );
        this._org.Db.SaveChanges();
    }

    [Fact]
    public async Task Invoice_FullLifecycle_UsesTheRightRoleAtEachStep()
    {
        Invoice invoice = null!;

        this.As(
            this._org.Sam,
            () =>
            {
                invoice = this._org.Sales.CreateInvoice( this._org.Fabrikam );
                invoice.Lines.Add( new InvoiceLine( "Consulting", 5, 1_000 ) );
            } );

        this.As( this._org.Mia, invoice.Approve );

        using ( this._org.SignIn( this._org.Sam ) )
        {
            await invoice.SendAsync( this._org.Outbox, CancellationToken.None );
        }

        this.As(
            this._org.Ada,
            () =>
            {
                invoice.RecordPayment( 3_000 );
                Assert.Equal( InvoiceStatus.Sent, invoice.Status );

                invoice.RecordPayment( 2_000 );
                Assert.Equal( InvoiceStatus.Paid, invoice.Status );
            } );
    }

    [Fact]
    public void CreateInvoice_MakesTheCreatorTheOwner_AndNumbersPerDepartment()
        => this.As(
            this._org.Mia,
            () =>
            {
                var sales = this._org.Sales.CreateInvoice( this._org.Fabrikam );
                var marketing = this._org.Marketing.CreateInvoice( this._org.Fabrikam );

                Assert.Equal( "SAL-0002", sales.Number );
                Assert.Equal( "MKT-0002", marketing.Number );
                Assert.Equal( this._org.Mia, sales.Owner );
                Assert.Equal( InvoiceStatus.Draft, sales.Status );
            } );

    [Fact]
    public void CreatorOfAnInvoice_CannotApproveIt_EvenAsManager()
    {
        Invoice invoice = null!;

        this.As(
            this._org.Mia,
            () =>
            {
                invoice = this._org.Sales.CreateInvoice( this._org.Fabrikam );
                invoice.Lines.Add( new InvoiceLine( "Training", 1, 900 ) );
            } );

        this.Denied( this._org.Mia, invoice.Approve );
    }

    [Fact]
    public void ApprovedInvoice_CannotBeEdited_ByBusinessRule_NotByAuthorization()
    {
        this.As( this._org.Mia, this._org.SalesInvoice.Approve );

        this.As( this._org.Mia, () => Assert.Throws<InvalidOperationException>( () => this._org.SalesInvoice.Lines.Add( new InvoiceLine( "Extra", 1, 1 ) ) ) );
    }

    [Fact]
    public async Task DeniedAsyncOperation_FaultsTheReturnedTask()
    {
        this.As( this._org.Mia, this._org.SalesInvoice.Approve );

        using ( this._org.SignIn( this._org.Otto ) )
        {
            // The check runs in the async method, so the exception is stored in the task.
            var task = this._org.SalesInvoice.SendAsync( this._org.Outbox, CancellationToken.None );

            await Assert.ThrowsAsync<AccessDeniedException>( () => task );
        }

        this.As( this._org.Mia, () => Assert.Equal( InvoiceStatus.Approved, this._org.SalesInvoice.Status ) );
    }

    [Fact]
    public void CreateInvoice_ForUnreadableCustomer_IsDeniedOnTheParameter()
        => this.As(
            this._org.Tom,
            () =>
            {
                var exception = Assert.Throws<AccessDeniedException>( () => this._org.Engineering.CreateInvoice( this._org.Fabrikam ) );

                Assert.Equal( this._org.Fabrikam.SecurityLabel, exception.Securable );
                Assert.Equal( "Department.CreateInvoice(customer)", exception.Member );
            } );

    [Fact]
    public void TransferTo_ForeignDepartment_IsDeniedOnTheTarget_AndChangesNothing()
    {
        this.As(
            this._org.Mia,
            () =>
            {
                var exception = Assert.Throws<AccessDeniedException>( () => this._org.SalesInvoice.TransferTo( this._org.Engineering ) );

                Assert.Equal( this._org.Engineering.SecurityLabel, exception.Securable );
                Assert.Equal( "Invoice.TransferTo(target)", exception.Member );
                Assert.Same( this._org.Sales, this._org.SalesInvoice.Department );
            } );
    }

    [Fact]
    public void TransferTo_ChangesWhoInheritsAccess()
    {
        var nina = new User( "Nina" );
        this.AsAndSave( this._org.Mia, () => this._org.Sales.AssignRole( nina, DemoRoles.DepartmentAssistant ) );
        this.As( nina, () => _ = this._org.SalesInvoice.Total );

        this.AsAndSave( this._org.Mia, () => this._org.SalesInvoice.TransferTo( this._org.Marketing ) );

        // Nina is a sales assistant, and the invoice now belongs to Marketing.
        this.Denied( nina, () => _ = this._org.SalesInvoice.Total );

        // Sam is a sales assistant too, but he still owns the invoice.
        this.As( this._org.Sam, () => _ = this._org.SalesInvoice.Total );

        // The marketing manager now sees the invoice.
        this.As( this._org.Grace, () => _ = this._org.SalesInvoice.Total );
    }

    [Fact]
    public void ChangingTheOwner_GivesTheNewOwnerAccess()
    {
        this.Denied( this._org.Vera, () => _ = this._org.SalesInvoice.Total );

        this.AsAndSave( this._org.Mia, () => this._org.SalesInvoice.Owner = this._org.Vera );

        // Vera has no role, but the owner of an invoice reads and edits it.
        this.As( this._org.Vera, () => this._org.SalesInvoice.Lines.Add( new InvoiceLine( "Expedite", 1, 50 ) ) );

        // A manager cannot change the owner of an invoice of a department she does not manage.
        this.Denied( this._org.Mia, () => this._org.EngineeringInvoice.Owner = this._org.Mia );
    }

    [Fact]
    public void AssignedRole_TakesEffectAfterTheSave()
    {
        var nina = new User( "Nina" );

        this.Denied( nina, () => _ = this._org.SalesInvoice.Total );
        this.As( this._org.Mia, () => this._org.Sales.AssignRole( nina, DemoRoles.DepartmentAssistant ) );
        this.As( nina, () => this._org.SalesInvoice.Lines.Add( new InvoiceLine( "Binding", 1, 20 ) ) );
    }

    [Fact]
    public void RevokedRole_TakesEffectAfterTheSave()
    {
        this.As( this._org.Erin, () => _ = this._org.EngineeringInvoice.Total );
        this.AsAndSave( this._org.Alice, () => this._org.Engineering.RevokeRole( this._org.Erin, DemoRoles.DepartmentManager ) );
        this.Denied( this._org.Erin, () => _ = this._org.EngineeringInvoice.Total );
    }

    [Fact]
    public void CompanyWideRole_AppliesToEveryDepartment()
    {
        var nina = new User( "Nina" );
        this.As( this._org.Alice, () => this._org.Contoso.AssignRole( nina, DemoRoles.Accountant ) );

        this.As(
            nina,
            () =>
            {
                this._org.SalesInvoice.Void( "Duplicate" );
                this._org.EngineeringInvoice.Void( "Duplicate" );
                _ = this._org.EngineeringBudget.Remaining;
            } );
    }

    [Fact]
    public void AuditorRole_AddedToAManager_RemovesTheirWriteAccess()
    {
        this.As( this._org.Erin, () => this._org.EngineeringInvoice.Lines.Add( new InvoiceLine( "Review", 1, 10 ) ) );
        this.AsAndSave( this._org.Alice, () => this._org.Contoso.AssignRole( this._org.Erin, DemoRoles.Auditor ) );
        this.Denied( this._org.Erin, () => this._org.EngineeringInvoice.Lines.Add( new InvoiceLine( "Review", 1, 10 ) ) );
    }

    [Fact]
    public void MakingOneselfTheOwner_IsDenied()
    {
        var exception = Assert.Throws<AccessDeniedException>(
            () => this.As( this._org.Mia, () => this._org.SalesInvoice.Owner = this._org.Mia ) );

        Assert.Contains( "cannot make themself the owner", exception.Message, StringComparison.Ordinal );
    }

    [Fact]
    public void CreatingAnInvoice_MakesTheCreatorItsOwner()
        => this.As(
            this._org.Sam,
            () =>
            {
                // The constructor sets the owner while the invoice is being initialized.
                var invoice = this._org.Sales.CreateInvoice( this._org.Fabrikam );

                Assert.Equal( this._org.Sam, invoice.Owner );
            } );

    [Fact]
    public void RevokingOneselfARole_IsDenied()
        => this.Denied( this._org.Mia, () => this._org.Sales.RevokeRole( this._org.Mia, DemoRoles.DepartmentManager ) );

    [Fact]
    public void ReassigningTheOwner_DoesNotBypassTheFourEyesPrinciple()
    {
        Invoice invoice = null!;

        this.As(
            this._org.Mia,
            () =>
            {
                invoice = this._org.Sales.CreateInvoice( this._org.Fabrikam );
                invoice.Lines.Add( new InvoiceLine( "Training", 1, 900 ) );

                // Mia gives the invoice to Sam, so she is no longer the owner, but she remains the creator.
                invoice.Owner = this._org.Sam;
            } );

        this.Denied( this._org.Mia, invoice.Approve );
    }
}
