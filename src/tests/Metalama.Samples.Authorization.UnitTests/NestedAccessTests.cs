using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

/// <summary>
/// The permission required by a member is not transitive: it does not grant the permissions of the members that
/// the member accesses. Each access is checked on its own, for the current subject.
/// </summary>
public class NestedAccessTests
{
    private static readonly Role _summarizer = new( "Summarizer" );
    private static readonly Role _publisher = new( "Publisher" );
    private static readonly Role _editor = new( "Editor" );

    private static readonly RoleBasedAuthorizationPolicy _policy = new RoleBasedAuthorizationPolicyBuilder()
        .Allow<Report>( "Summarize", _summarizer )
        .Allow<Report>( "Publish", _publisher )
        .Allow<Report>( Read, _editor )
        .Allow<Report>( Write, _editor )
        .Build();

    private readonly TestUser _summarizerOnly = new( "summarizer" );
    private readonly TestUser _publisherOnly = new( "publisher" );
    private readonly TestUser _publisherAndEditor = new( "publisher and editor" );
    private readonly Report _report;

    public NestedAccessTests()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            this._report = new Report { Body = "Quarterly results" };
            this._report.SetDraftNotes( "Check the figures" );
            this._report.AssignRole( this._summarizerOnly, _summarizer );
            this._report.AssignRole( this._publisherOnly, _publisher );
            this._report.AssignRole( this._publisherAndEditor, _publisher );
            this._report.AssignRole( this._publisherAndEditor, _editor );
        }
    }

    private IDisposable SignIn( ISubject subject ) => SecurityContext.Use( new UserSecurityContext( subject, _policy ) );

    [Fact]
    public void Aggregate_OverASecuredDetail_RequiresThePermissionOfTheDetail()
    {
        using ( this.SignIn( this._summarizerOnly ) )
        {
            // The subject may read BodyLength, but BodyLength reads Body, which requires Read.
            var exception = Assert.Throws<AccessDeniedException>( () => this._report.BodyLength );

            Assert.Equal( "Report.Body", exception.Member );
            Assert.Equal( Read, exception.Permission.Name );
        }
    }

    [Fact]
    public void Aggregate_OverPrivateState_RequiresOnlyItsOwnPermission()
    {
        // Private members are never secured, so the aggregate is computed without the permission of the details.
        using ( this.SignIn( this._summarizerOnly ) )
        {
            Assert.Equal( 17, this._report.DraftNotesLength );
        }
    }

    [Fact]
    public void Method_ThatReadsAndWritesAProperty_RequiresThePermissionsOfTheProperty()
    {
        using ( this.SignIn( this._publisherOnly ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( this._report.Publish );

            // The method was entered: the denial comes from the first access to Body.
            Assert.Equal( "Report.Body", exception.Member );
            Assert.Equal( AccessKind.Read, exception.AccessKind );
        }
    }

    [Fact]
    public void Method_WithAllThePermissions_Succeeds()
    {
        using ( this.SignIn( this._publisherAndEditor ) )
        {
            this._report.Publish();

            Assert.Equal( "Quarterly results (published)", this._report.Body );
        }
    }

    [Fact]
    public void EntryPermission_IsStillChecked_WhenTheDetailsAreAllowed()
    {
        // Read and Write on Body do not grant Publish.
        var editorOnly = new TestUser( "editor" );

        using ( SecurityContext.RunAsSystem() )
        {
            this._report.AssignRole( editorOnly, _editor );
        }

        using ( this.SignIn( editorOnly ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( this._report.Publish );

            Assert.Equal( "Report.Publish", exception.Member );
        }
    }
}
