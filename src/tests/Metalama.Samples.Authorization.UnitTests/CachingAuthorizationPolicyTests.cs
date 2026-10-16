using Metalama.Samples.Authorization.RoleBased;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

/// <summary>
/// <see cref="CachingAuthorizationPolicy"/> caches the decisions of a policy for all execution contexts, behind the
/// cache of each execution context, until they are invalidated manually.
/// </summary>
public sealed class CachingAuthorizationPolicyTests : IDisposable
{
    private static readonly Role _reader = new( "Reader" );

    private readonly MemoryCache _memoryCache = new( new MemoryCacheOptions() );
    private readonly CountingPolicy _inner;
    private readonly CachingAuthorizationPolicy _policy;
    private readonly TestUser _user = new( "reader" );
    private readonly Folder _parent;
    private readonly Folder _folder;
    private readonly Folder _unsavedFolder;
    private readonly Folder _otherFolder;
    private CachingAuthorizationPolicy? _policyUnderTest;

    public CachingAuthorizationPolicyTests()
    {
        this._inner = new CountingPolicy( new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Read, _reader ).Build() );

        // In these tests, the folders and the user are their own keys, except the folder that is not saved yet.
        this._policy = new CachingAuthorizationPolicy(
            this._inner,
            this._memoryCache,
            value => value is TestUser || (value is Folder folder && folder != this._unsavedFolder) ? value : null,
            GetParents );

        using ( SecurityContext.RunAsSystem() )
        {
            this._parent = new Folder { Title = "Parent" };
            this._folder = new Folder { Parent = this._parent, Title = "Child" };
            this._unsavedFolder = new Folder { Title = "Unsaved" };
            this._otherFolder = new Folder { Title = "Other" };
            this._parent.AssignRole( this._user, _reader );
        }

        this.Load();
    }

    // Records the load of the folders, like a request that loads them from the database.
    private void Load()
    {
        this._policy.RecordLoad( this._parent );
        this._policy.RecordLoad( this._folder );
        this._policy.RecordLoad( this._otherFolder );
    }

    public void Dispose()
    {
        this._policy.Dispose();
        this._memoryCache.Dispose();
    }

    private static IEnumerable<ISecurable> GetParents( ISecurable securable )
    {
        for ( var parent = (securable as IRoleBasedSecurable)?.SecurityParent; parent != null; parent = parent.SecurityParent )
        {
            yield return parent;
        }
    }

    private void ReadTitleOf( Folder folder )
    {
        using ( SecurityContext.Use( new UserSecurityContext( this._user, this._policy ) ) )
        {
            _ = folder.Title;
        }
    }

    // Each call is a new execution context, like a new request.
    private string? ReadTitle( Folder folder )
    {
        using ( SecurityContext.Use( new UserSecurityContext( this._user, this._policy ) ) )
        {
            return folder.Title;
        }
    }

    [Fact]
    public void Decision_IsSharedByExecutionContexts()
    {
        this.ReadTitle( this._folder );
        this.ReadTitle( this._folder );

        Assert.Equal( 1, this._inner.Evaluations );
    }

    [Fact]
    public void ObjectWithoutKey_IsNotCached()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            this._unsavedFolder.AssignRole( this._user, _reader );
        }

        this.ReadTitle( this._unsavedFolder );
        this.ReadTitle( this._unsavedFolder );

        Assert.Equal( 2, this._inner.Evaluations );
    }

    [Fact]
    public void ObjectLoadedBeforeAnInvalidation_IsNotCached()
    {
        // Another request saves a change of another object. The folder was loaded before, so its data may be out of
        // date, and the decisions made from it are not shared with the other requests.
        this._policy.Invalidate( this._otherFolder );

        this.ReadTitle( this._folder );
        this.ReadTitle( this._folder );

        Assert.Equal( 2, this._inner.Evaluations );

        // A request that loads the folder again makes decisions that can be shared.
        this.Load();
        this.ReadTitle( this._folder );
        this.ReadTitle( this._folder );

        Assert.Equal( 3, this._inner.Evaluations );
    }

    [Fact]
    public void ObjectWhoseLoadIsNotRecorded_IsNotCached()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            var folder = new Folder { Parent = this._parent };

            this.ReadTitleOf( folder );
            this.ReadTitleOf( folder );
        }

        Assert.Equal( 2, this._inner.Evaluations );
    }

    [Fact]
    public void WithoutInvalidation_TheChangeIsNotSeen()
    {
        this.ReadTitle( this._folder );

        using ( SecurityContext.RunAsSystem() )
        {
            this._parent.RevokeRole( this._user, _reader );
        }

        Assert.Equal( "Child", this.ReadTitle( this._folder ) );
    }

    [Fact]
    public void InvalidatingAParent_InvalidatesItsChildren()
    {
        this.ReadTitle( this._folder );

        using ( SecurityContext.RunAsSystem() )
        {
            this._parent.RevokeRole( this._user, _reader );
        }

        this._policy.Invalidate( this._parent );
        this.Load();

        Assert.Throws<AccessDeniedException>( () => this.ReadTitle( this._folder ) );
    }

    [Fact]
    public void InvalidatingAChild_KeepsTheDecisionsOnItsParent()
    {
        this.ReadTitle( this._parent );
        this.ReadTitle( this._folder );

        this._policy.Invalidate( this._folder );
        this.Load();

        this.ReadTitle( this._parent );
        this.ReadTitle( this._folder );

        Assert.Equal( 3, this._inner.Evaluations );
    }

    [Fact]
    public void InvalidateAll_RemovesAllDecisions()
    {
        this.ReadTitle( this._parent );
        this.ReadTitle( this._folder );

        this._policy.InvalidateAll();
        this.Load();

        this.ReadTitle( this._parent );
        this.ReadTitle( this._folder );

        Assert.Equal( 4, this._inner.Evaluations );
    }

    [Fact]
    public void Invalidation_WhileThePolicyRuns_IsNotLost()
    {
        var policy = new CachingAuthorizationPolicy(
            new InvalidatingPolicy( this._inner, () => this._policyUnderTest!.Invalidate( this._folder ) ),
            this._memoryCache,
            value => value,
            GetParents );

        this._policyUnderTest = policy;
        policy.RecordLoad( this._folder );
        policy.RecordLoad( this._parent );

        using ( SecurityContext.RunAsSystem() )
        {
            policy.IsGranted( this._user, Read, this._folder );
            policy.IsGranted( this._user, Read, this._folder );
        }

        // The first decision was computed while the folder was invalidated, so it was not kept.
        Assert.Equal( 2, this._inner.Evaluations );
    }

    // A policy that invalidates the cache the first time it runs, like a change saved by another request while the
    // policy reads the data.
    private sealed class InvalidatingPolicy( IAuthorizationPolicy inner, Action invalidate ) : IAuthorizationPolicy
    {
        private bool _hasInvalidated;

        public bool IsGranted( ISubject subject, Permission permission, ISecurable securable )
        {
            var isGranted = inner.IsGranted( subject, permission, securable );

            if ( !this._hasInvalidated )
            {
                this._hasInvalidated = true;
                invalidate();
            }

            return isGranted;
        }
    }
}
