using Metalama.Samples.Authorization.RoleBased;
using Xunit;

namespace Metalama.Samples.Authorization.UnitTests;

public class AccessGuardTests
{
    private static readonly RoleBasedAuthorizationPolicy _everyoneMayRead =
        new RoleBasedAuthorizationPolicy().Allow<Folder>( PermissionNames.Read, Role.Everyone );

    private readonly TestUser _alice = new( "alice" );
    private readonly RecordingAuditor _auditor = new();

    private IDisposable SignIn() => SecurityContext.Use( new UserSecurityContext( this._alice, _everyoneMayRead, this._auditor ) );

    [Fact]
    public void Denial_IsAuditedOnce_WithFullDetails()
    {
        var folder = new Folder();

        using ( this.SignIn() )
        {
            var thrown = Assert.Throws<AccessDeniedException>( () => folder.Title = "Inbox" );

            var audited = Assert.Single( this._auditor.Denials );
            Assert.Same( thrown, audited );
            Assert.Equal( this._alice, audited.Subject );
            Assert.Equal( PermissionNames.Write, audited.Permission.Name );
            Assert.Same( folder, audited.Securable );
            Assert.Equal( "Folder.Title", audited.Member );
            Assert.Equal( AccessKind.Write, audited.AccessKind );
        }
    }

    [Fact]
    public void GrantedAccess_IsNotAudited()
    {
        var folder = new Folder();

        using ( this.SignIn() )
        {
            _ = folder.Title;
        }

        Assert.Empty( this._auditor.Denials );
    }

    [Fact]
    public void Message_DescribesTheDenial()
    {
        using ( this.SignIn() )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => new Folder().Title = "Inbox" );

            Assert.Equal( "Cannot write Folder.Title: 'alice' does not have the Write permission on 'folder'.", exception.Message );
        }
    }

    [Fact]
    public void Auditor_RunsInSystemScope()
    {
        var auditor = new ProbeAuditor();
        var context = new UserSecurityContext( this._alice, new RoleBasedAuthorizationPolicy(), auditor );

        using ( SecurityContext.Use( context ) )
        {
            Assert.Throws<AccessDeniedException>( () => new Folder().Title );
        }

        Assert.True( auditor.WasSystem );
    }

    private sealed class RecordingAuditor : IAccessAuditor
    {
        private readonly List<AccessDeniedException> _denials = [];

        public IReadOnlyList<AccessDeniedException> Denials => this._denials;

        public void OnAccessDenied( AccessDeniedException exception ) => this._denials.Add( exception );
    }

    private sealed class ProbeAuditor : IAccessAuditor
    {
        public bool WasSystem { get; private set; }

        public void OnAccessDenied( AccessDeniedException exception ) => this.WasSystem = SecurityContext.IsSystem;
    }
}
