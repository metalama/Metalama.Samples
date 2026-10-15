using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

public class GuardedListTests
{
    private static readonly RoleBasedAuthorizationPolicy _policy = new RoleBasedAuthorizationPolicyBuilder()
        .Allow<object>( Read, Role.Everyone )
        .Allow<object>( Write, TestRoles.Editor )
        .Build();

    private readonly TestUser _reader = new( "reader" );
    private readonly TestUser _editor = new( "editor" );
    private readonly Folder _folder;

    public GuardedListTests()
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
    public void NewList_HasNoOwner_AndIsNotChecked()
    {
        // Before the first access through the property, the list belongs to nobody: the code that creates the
        // object, or Entity Framework, fills it without a security context.
        var list = new GuardedList<string> { "a", "b" };

        Assert.Null( list.Owner );
        Assert.Equal( 2, list.Count );
    }

    [Fact]
    public void FirstAccess_SetsTheOwner_AndReturnsTheSameList()
    {
        using ( this.SignIn( this._reader ) )
        {
            var tags = this._folder.Tags;

            Assert.Same( tags, this._folder.Tags );
            Assert.Same( this._folder, Assert.IsType<GuardedList<string>>( tags ).Owner );
            Assert.Equal( ["draft"], tags );
        }
    }

    [Fact]
    public void Owner_CanBeSetOnlyOnce()
    {
        var list = new GuardedList<string>();
        var first = new Folder();

        list.SetOwner( first, Write, "Folder.Tags", "Tags" );

        // Setting the same owner again has no effect.
        list.SetOwner( first, Write, "Folder.Tags", "Tags" );

        Assert.Throws<InvalidOperationException>( () => list.SetOwner( new Folder(), Write, "Folder.Tags", "Tags" ) );
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
    public void Editor_ChangesTheList()
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
    public void List_ChecksTheSubjectOfTheChange_NotTheOneThatReadTheProperty()
    {
        IList<string> tags;

        using ( this.SignIn( this._editor ) )
        {
            tags = this._folder.Tags;
        }

        using ( this.SignIn( this._reader ) )
        {
            Assert.Throws<AccessDeniedException>( () => tags.Add( "final" ) );
        }
    }

    [Fact]
    public void ReadOnlyProperty_CastBackToIList_StillRequiresWrite()
    {
        using ( this.SignIn( this._reader ) )
        {
            var history = (IList<string>) this._folder.History;

            Assert.Throws<AccessDeniedException>( () => history.Add( "edited" ) );
        }
    }

    [Fact]
    public void SequenceThatIsNotACollection_IsReturnedAsItIs()
    {
        using ( this.SignIn( this._reader ) )
        {
            Assert.Equal( ["DRAFT"], this._folder.UpperTags );
        }
    }

    [Fact]
    public void OrdinaryList_MakesTheGetterFailClosed()
    {
        var folder = new UnguardedFolder();

        using ( this.SignIn( this._reader ) )
        {
            var exception = Assert.Throws<InvalidOperationException>( () => folder.Tags );

            Assert.Contains( "GuardedList", exception.Message, StringComparison.Ordinal );
        }
    }

    [Fact]
    public void OwnerHook_IsCalledAfterThePermission_AndCanCancelTheChange()
    {
        var folder = new ArchivableFolder();

        using ( SecurityContext.RunAsSystem() )
        {
            folder.AssignRole( this._editor, TestRoles.Editor );
        }

        // Without the permission, the hook is not called.
        using ( this.SignIn( this._reader ) )
        {
            Assert.Throws<AccessDeniedException>( () => folder.Tags.Add( "a" ) );
        }

        Assert.Empty( folder.ChangedProperties );

        using ( this.SignIn( this._editor ) )
        {
            folder.Tags.Add( "a" );
            Assert.Equal( ["Tags"], folder.ChangedProperties );

            using ( SecurityContext.RunAsSystem() )
            {
                folder.IsArchived = true;
            }

            Assert.Throws<InvalidOperationException>( () => folder.Tags.Add( "b" ) );
            Assert.Equal( ["a"], folder.Tags );
        }
    }
}
