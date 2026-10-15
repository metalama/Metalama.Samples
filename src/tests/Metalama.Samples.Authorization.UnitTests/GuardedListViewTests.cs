using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

public class GuardedListViewTests
{
    private static readonly RoleBasedAuthorizationPolicy _policy = new RoleBasedAuthorizationPolicyBuilder()
        .Allow<Folder>( Read, Role.Everyone )
        .Allow<Folder>( Write, TestRoles.Editor )
        .Build();

    private readonly TestUser _reader = new( "reader" );
    private readonly TestUser _editor = new( "editor" );
    private readonly Folder _folder;

    public GuardedListViewTests()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            this._folder = new Folder();
            this._folder.AssignRole( this._editor, TestRoles.Editor );
            this._folder.Tags.Add( "draft" );
        }
    }

    private IDisposable SignIn( ISubject subject ) => SecurityContext.Use( new UserSecurityContext( subject, _policy ) );

    [Fact]
    public void Reader_ReadsTheList()
    {
        using ( this.SignIn( this._reader ) )
        {
            var tags = this._folder.Tags;

            Assert.Equal( ["draft"], tags );
            Assert.Equal( "draft", tags[0] );
            Assert.True( tags.Contains( "draft" ) );
        }
    }

    [Fact]
    public void Reader_CannotChangeTheList()
    {
        using ( this.SignIn( this._reader ) )
        {
            var tags = this._folder.Tags;

            Assert.Throws<AccessDeniedException>( () => tags.Add( "final" ) );
            Assert.Throws<AccessDeniedException>( () => tags.Insert( 0, "final" ) );
            Assert.Throws<AccessDeniedException>( () => tags[0] = "final" );
            Assert.Throws<AccessDeniedException>( () => tags.Remove( "draft" ) );
            Assert.Throws<AccessDeniedException>( () => tags.RemoveAt( 0 ) );
            Assert.Throws<AccessDeniedException>( tags.Clear );

            var exception = Assert.Throws<AccessDeniedException>( () => tags.Add( "final" ) );
            Assert.Equal( "Folder.Tags", exception.Member );
            Assert.Equal( AccessKind.Write, exception.AccessKind );
        }

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( ["draft"], this._folder.Tags );
        }
    }

    [Fact]
    public void Reader_CannotCastTheViewToTheList()
    {
        using ( this.SignIn( this._reader ) )
        {
            Assert.IsNotType<List<string>>( this._folder.Tags );
        }
    }

    [Fact]
    public void Editor_ChangesTheListOfTheOwner()
    {
        using ( this.SignIn( this._editor ) )
        {
            this._folder.Tags.Add( "final" );
            this._folder.Tags.Remove( "draft" );
        }

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( ["final"], this._folder.Tags );
        }
    }

    [Fact]
    public void View_ChecksTheCurrentSubject_NotTheOneThatReadTheProperty()
    {
        IList<string> tags;

        using ( this.SignIn( this._editor ) )
        {
            tags = this._folder.Tags;
        }

        // The view demands the permission when the list changes, for the subject of that moment.
        using ( this.SignIn( this._reader ) )
        {
            Assert.Throws<AccessDeniedException>( () => tags.Add( "final" ) );
        }
    }
}
