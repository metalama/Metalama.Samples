using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

public class SecurableListTests
{
    private static readonly RoleBasedAuthorizationPolicy _policy = new RoleBasedAuthorizationPolicyBuilder()
        .Allow<object>( Read, Role.Everyone )
        .Allow<object>( Write, TestRoles.Editor )
        .Build();

    private readonly TestUser _reader = new( "reader" );
    private readonly TestUser _editor = new( "editor" );
    private readonly Folder _folder;

    public SecurableListTests()
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
        var list = new SecurableList<string> { "a", "b" };

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
            Assert.Same( this._folder, Assert.IsType<SecurableList<string>>( tags ).Owner );
            Assert.Equal( ["draft"], tags );
        }
    }

    [Fact]
    public void Owner_CanBeSetOnlyOnce()
    {
        var list = new SecurableList<string>();
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

            // Each change is denied with its own kind of access, and the write permission of the property.
            AssertDenied( AccessKind.Add, () => tags.Add( "final" ) );
            AssertDenied( AccessKind.Add, () => tags.Insert( 0, "final" ) );
            AssertDenied( AccessKind.Replace, () => tags[0] = "final" );
            AssertDenied( AccessKind.Remove, () => tags.Remove( "draft" ) );
            AssertDenied( AccessKind.Remove, () => tags.RemoveAt( 0 ) );
            AssertDenied( AccessKind.Remove, tags.Clear );

            var exception = Assert.Throws<AccessDeniedException>( () => tags.Add( "final" ) );
            Assert.Equal( "Folder.Tags", exception.Member );
            Assert.StartsWith( "Cannot add an item to Folder.Tags:", exception.Message, StringComparison.Ordinal );
        }

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( ["draft"], this._folder.Tags );
        }
    }

    private static void AssertDenied( AccessKind accessKind, Action change )
    {
        var exception = Assert.Throws<AccessDeniedException>( change );

        Assert.Equal( accessKind, exception.AccessKind );
        Assert.Equal( Write, exception.Permission.Name );
        Assert.True( AccessKind.Change.HasFlag( exception.AccessKind ) );
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
        var folder = new MutableListFolder();

        using ( this.SignIn( this._reader ) )
        {
            var exception = Assert.Throws<InvalidOperationException>( () => folder.Tags );

            Assert.Contains( "SecurableList", exception.Message, StringComparison.Ordinal );
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
            folder.Tags[0] = "b";
            folder.Tags.Remove( "b" );
            folder.Tags.Add( "a" );
            Assert.Equal( ["Tags", "Tags", "Tags", "Tags"], folder.ChangedProperties );
            Assert.Equal( [AccessKind.Add, AccessKind.Replace, AccessKind.Remove, AccessKind.Add], folder.ChangeKinds );

            using ( SecurityContext.RunAsSystem() )
            {
                folder.IsArchived = true;
            }

            Assert.Throws<InvalidOperationException>( () => folder.Tags.Add( "b" ) );
            Assert.Equal( ["a"], folder.Tags );
        }
    }
}
