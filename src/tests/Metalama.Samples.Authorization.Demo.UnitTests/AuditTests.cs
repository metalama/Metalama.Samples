using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

public class AuditTests : IAsyncLifetime
{
    private DemoOrganization _org = null!;

    public async Task InitializeAsync() => this._org = await DemoOrganization.CreateAsync();

    public async Task DisposeAsync() => await this._org.DisposeAsync();
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
            Assert.Same( this._org.SalesInvoice, audited.Securable );
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
                "Cannot read Invoice.Total: 'Tom' does not have the Read permission on 'invoice SAL-0001'.",
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
        Assert.Same( this._org.Engineering, audited.Securable );
        Assert.Equal( "Invoice.TransferTo(target)", audited.Member );
    }
}
