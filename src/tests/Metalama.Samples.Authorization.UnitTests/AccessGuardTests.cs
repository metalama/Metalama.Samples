using Metalama.Samples.Authorization.RoleBased;
using Xunit;

namespace Metalama.Samples.Authorization.UnitTests;

public class AccessGuardTests
{
    private static readonly RoleBasedAuthorizationPolicy _everyoneMayRead =
        new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( PermissionNames.Read, Role.Everyone ).Build();

    private readonly TestUser _alice = new( "alice" );
    private readonly RecordingAuditor _auditor = new();

    private IDisposable SignIn( IAuthorizationPolicy? policy = null )
        => SecurityContext.Use( new UserSecurityContext( this._alice, policy ?? _everyoneMayRead, this._auditor ) );

    [Fact]
    public void Denial_IsAuditedOnce_WithFullDetails()
    {
        var folder = new Folder();

        using ( this.SignIn() )
        {
            var thrown = Assert.Throws<AccessDeniedException>( () => folder.Title = "Inbox" );

            var audited = Assert.Single( this._auditor.Denials );
            Assert.Same( thrown, audited );
            Assert.Equal( new AccessRecord( this._alice, PermissionNames.Write, "Folder", "Folder.Title", AccessKind.Write ), audited.Record );
        }
    }

    [Fact]
    public void GrantedAccess_IsNotAudited_UnlessTheAuditorSelectsThePermission()
    {
        var folder = new Folder();

        using ( this.SignIn() )
        {
            _ = folder.Title;
        }

        Assert.Empty( this._auditor.Denials );
        Assert.Empty( this._auditor.Grants );

        this._auditor.AuditedGrants.Add( PermissionNames.Read );

        using ( this.SignIn() )
        {
            _ = folder.Title;
        }

        Assert.Equal( new AccessRecord( this._alice, PermissionNames.Read, "Folder", "Folder.Title", AccessKind.Read ), Assert.Single( this._auditor.Grants ) );
    }

    [Fact]
    public void Message_NamesTheObjectByItsLabelOnly()
    {
        var folder = new LabeledFolder { Name = "Secret plans" };

        using ( this.SignIn() )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => folder.Title = "Inbox" );

            Assert.Equal( "Cannot write Folder.Title: 'alice' does not have the Write permission on Folder #7.", exception.Message );
            Assert.DoesNotContain( "Secret", exception.Message, StringComparison.Ordinal );
        }
    }

    [Fact]
    public void Exception_DoesNotReferenceTheSecurableObject()
    {
        // A logger that serializes the exception must not be able to reach the object and its secured members.
        var properties = typeof(AccessDeniedException).GetProperties().Concat( typeof(AccessRecord).GetProperties() );

        Assert.DoesNotContain( properties, p => p.PropertyType == typeof(object) || typeof(IRoleBasedSecurable).IsAssignableFrom( p.PropertyType ) );
    }

    [Fact]
    public void Auditor_RunsOutsideOfTheSystemScope()
    {
        var auditor = new ProbeAuditor();
        var context = new UserSecurityContext( this._alice, new RoleBasedAuthorizationPolicyBuilder().Build(), auditor );

        using ( SecurityContext.Use( context ) )
        {
            Assert.Throws<AccessDeniedException>( () => new Folder().Title );
        }

        Assert.True( auditor.WasCalled );
        Assert.False( auditor.WasSystem );
    }

    [Fact]
    public void Deny_ThrowsAndAudits_WithTheReason()
    {
        using ( this.SignIn() )
        {
            var exception = Assert.Throws<AccessDeniedException>(
                () => AccessGuard.Deny( new Folder(), PermissionNames.ManageRoles, "Folder.Share", "sharing is disabled" ) );

            Assert.Equal( "Cannot execute Folder.Share: sharing is disabled.", exception.Message );
            Assert.Same( exception, Assert.Single( this._auditor.Denials ) );
        }
    }

    private sealed class RecordingAuditor : IAccessAuditor
    {
        public List<AccessDeniedException> Denials { get; } = [];

        public List<AccessRecord> Grants { get; } = [];

        public HashSet<string> AuditedGrants { get; } = [];

        public void OnAccessDenied( AccessDeniedException exception ) => this.Denials.Add( exception );

        public bool AuditsGrantsOf( Permission permission ) => this.AuditedGrants.Contains( permission.Name );

        public void OnAccessGranted( AccessRecord record ) => this.Grants.Add( record );
    }

    private sealed class ProbeAuditor : IAccessAuditor
    {
        public bool WasCalled { get; private set; }

        public bool WasSystem { get; private set; }

        public void OnAccessDenied( AccessDeniedException exception )
        {
            this.WasCalled = true;
            this.WasSystem = SecurityContext.IsSystem;
        }
    }
}
