using Metalama.Samples.Authorization.RoleBased;
using Xunit;

namespace Metalama.Samples.Authorization.UnitTests;

public class SecurityContextTests
{
    private static readonly RoleBasedAuthorizationPolicy _everyoneMayRead =
        new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( PermissionNames.Read, Role.Everyone ).Build();

    [Fact]
    public void WithoutContext_SecuredAccessFails()
    {
        var folder = new Folder();

        var exception = Assert.Throws<InvalidOperationException>( () => folder.Title );
        Assert.Contains( "Folder.Title", exception.Message, StringComparison.Ordinal );
    }

    [Fact]
    public void RunAsSystem_BypassesChecks_UntilDisposed()
    {
        var folder = new Folder();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.True( SecurityContext.IsSystem );
            folder.Title = "Inbox";
        }

        Assert.False( SecurityContext.IsSystem );
        Assert.Throws<InvalidOperationException>( () => folder.Title );
    }

    [Fact]
    public void RunAsSystem_Nested_RestoresOuterScope()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            using ( SecurityContext.RunAsSystem() ) { }

            Assert.True( SecurityContext.IsSystem );
        }

        Assert.False( SecurityContext.IsSystem );
    }

    [Fact]
    public void Use_RestoresPreviousContext()
    {
        var outer = new UserSecurityContext( new TestUser( "outer" ), _everyoneMayRead );
        var inner = new UserSecurityContext( new TestUser( "inner" ), _everyoneMayRead );

        using ( SecurityContext.Use( outer ) )
        {
            using ( SecurityContext.Use( inner ) )
            {
                Assert.Same( inner, SecurityContext.Current );
            }

            Assert.Same( outer, SecurityContext.Current );
        }

        Assert.Null( SecurityContext.Current );
    }

    [Fact]
    public async Task Context_FlowsAcrossAwait()
    {
        var context = new UserSecurityContext( new TestUser( "alice" ), _everyoneMayRead );

        using ( SecurityContext.Use( context ) )
        {
            await Task.Yield();

            Assert.Same( context, SecurityContext.Current );
        }
    }

    [Fact]
    public async Task Context_IsIsolatedBetweenConcurrentOperations()
    {
        var bothSignedIn = new Barrier( 2 );

        async Task<string?> RunAs( string name )
        {
            await Task.Yield();

            using ( SecurityContext.Use( new UserSecurityContext( new TestUser( name ), _everyoneMayRead ) ) )
            {
                // Wait until the other operation has set its own context.
                bothSignedIn.SignalAndWait( TimeSpan.FromSeconds( 10 ) );

                return SecurityContext.Current?.Subject.Name;
            }
        }

        var results = await Task.WhenAll( Task.Run( () => RunAs( "alice" ) ), Task.Run( () => RunAs( "bob" ) ) );

        Assert.Equal( "alice", results[0] );
        Assert.Equal( "bob", results[1] );
    }

    [Fact]
    public void Policy_RunsInSystemScope_WithoutRecursion()
    {
        var policy = new ProbePolicy();
        var folder = new Folder();

        using ( SecurityContext.Use( new UserSecurityContext( new TestUser( "alice" ), policy ) ) )
        {
            _ = folder.Title;
        }

        Assert.True( policy.WasSystemWhileEvaluating );
    }

    [Fact]
    public void DeniedAccess_ThrowsAccessDeniedException()
    {
        var folder = new Folder();

        using ( SecurityContext.Use( new UserSecurityContext( new TestUser( "alice" ), _everyoneMayRead ) ) )
        {
            _ = folder.Title;

            var exception = Assert.Throws<AccessDeniedException>( () => folder.Title = "Inbox" );
            Assert.Equal( PermissionNames.Write, exception.Permission.Name );
        }
    }
}
