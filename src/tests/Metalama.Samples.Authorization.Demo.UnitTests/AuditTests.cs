using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

public class AuditTests : IAsyncLifetime
{
    private DemoOrganization _org = null!;

    public async ValueTask InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async ValueTask DisposeAsync() => await this._org.DisposeAsync();
    private readonly RecordingAuditor _auditor = new();

    [Fact]
    public void Denial_IsAuditedOnce_WithFullDetails()
    {
        using ( this._org.SignIn( this._org.Sam, this._auditor ) )
        {
            var thrown = Assert.Throws<AccessDeniedException>( this._org.SalesInvoice.Approve );

            var audited = Assert.Single( this._auditor.Denials );
            Assert.Same( thrown, audited );
            Assert.Equal( this._org.Sam, audited.Subject );
            Assert.Equal( DemoPermissions.Approve, audited.Permission.Name );
            Assert.Equal( this._org.SalesInvoice.SecurityLabel, audited.Securable );
            Assert.Equal( "Invoice.Approve", audited.Member );
            Assert.Equal( AccessKind.Execute, audited.AccessKind );
        }
    }

    [Fact]
    public void GrantedAccess_IsNotAudited()
    {
        using ( this._org.SignIn( this._org.Sam, this._auditor ) )
        {
            _ = this._org.SalesInvoice.Total;
        }

        Assert.Empty( this._auditor.Denials );
    }

    [Fact]
    public void Message_DescribesTheDenial()
    {
        using ( this._org.SignIn( this._org.Tom, this._auditor ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => _ = this._org.SalesInvoice.Total );

            Assert.Equal(
                $"Cannot read Invoice.Total: 'Tom' does not have the Read permission on {this._org.SalesInvoice.SecurityLabel}.",
                exception.Message );
        }
    }

    [Fact]
    public void Write_IsAuditedAsWrite()
    {
        using ( this._org.SignIn( this._org.Otto, this._auditor ) )
        {
            Assert.Throws<AccessDeniedException>( () => this._org.Fabrikam.Email = "otto@contoso.example" );
        }

        var audited = Assert.Single( this._auditor.Denials );
        Assert.Equal( AccessKind.Write, audited.AccessKind );
        Assert.Equal( "Customer.Email", audited.Member );
    }

    [Fact]
    public void ParameterCheck_IsAuditedOnTheArgument()
    {
        using ( this._org.SignIn( this._org.Mia, this._auditor ) )
        {
            Assert.Throws<AccessDeniedException>( () => this._org.SalesInvoice.TransferTo( this._org.Engineering ) );
        }

        var audited = Assert.Single( this._auditor.Denials );
        Assert.Equal( this._org.Engineering.SecurityLabel, audited.Securable );
        Assert.Equal( "Invoice.TransferTo(target)", audited.Member );
    }

    [Fact]
    public void RoleAssignment_IsAuditedAsAGrant()
    {
        this._auditor.AuditedGrants.Add( PermissionNames.ManageRoles );

        using ( this._org.SignIn( this._org.Mia, this._auditor ) )
        {
            this._org.Sales.AssignRole( new User( "Nina" ), DemoRoles.DepartmentAssistant );
        }

        var grant = Assert.Single( this._auditor.Grants );
        Assert.Equal( this._org.Mia, grant.Subject );
        Assert.Equal( "RoleBasedSecurable.AssignRole", grant.Member );
        Assert.Equal( this._org.Sales.SecurityLabel, grant.Securable );
    }

    [Fact]
    public void SelfAssignment_IsDeniedAndAudited()
    {
        using ( this._org.SignIn( this._org.Alice, this._auditor ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => this._org.Contoso.AssignRole( this._org.Alice, DemoRoles.Accountant ) );

            Assert.Equal( "Cannot execute RoleBasedSecurable.AssignRole: 'Alice' cannot assign a role to themself.", exception.Message );
            Assert.Same( exception, Assert.Single( this._auditor.Denials ) );
        }
    }

    [Fact]
    public void Message_DoesNotDiscloseTheNameOfAnUnreadableCustomer()
    {
        using ( this._org.SignIn( this._org.Tom, this._auditor ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => this._org.Fabrikam.Email );

            Assert.DoesNotContain( "Fabrikam", exception.Message, StringComparison.Ordinal );
        }
    }
}
