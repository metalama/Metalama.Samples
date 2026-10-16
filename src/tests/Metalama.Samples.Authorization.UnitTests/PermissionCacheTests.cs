using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

/// <summary>
/// Each execution context caches the decisions of the policy. <see cref="SecurityContext.Use"/> and
/// <see cref="SecurityContext.Grant"/> start an execution context; all the code that runs in it shares its cache.
/// </summary>
public class PermissionCacheTests
{
    private static readonly Role _reader = new( "Reader" );
    private static readonly Role _summarizer = new( "Summarizer" );

    private readonly CountingPolicy _policy = new(
        new RoleBasedAuthorizationPolicyBuilder()
            .Allow<Report>( Read, _reader )
            .Allow<Report>( "Summarize", _summarizer )
            .Build() );

    private readonly TestUser _readerUser = new( "reader" );
    private readonly TestUser _summarizerOnly = new( "summarizer" );
    private readonly Report _report;

    public PermissionCacheTests()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            this._report = new Report { Body = "Quarterly results" };
            this._report.AssignRole( this._readerUser, _reader );
            this._report.AssignRole( this._summarizerOnly, _summarizer );
        }
    }

    private SecurityScope SignIn( ISubject subject, IAccessAuditor? auditor = null )
        => SecurityContext.Use( new UserSecurityContext( subject, this._policy, auditor ) );

    [Fact]
    public void RepeatedAccess_EvaluatesThePolicyOnce()
    {
        using ( this.SignIn( this._readerUser ) )
        {
            _ = this._report.Body;
            _ = this._report.Body;
        }

        Assert.Equal( 1, this._policy.Evaluations );
    }

    [Fact]
    public async Task DecisionOfAnAsyncMethod_IsVisibleToItsCaller()
    {
        async Task ReadAsync()
        {
            await Task.Yield();
            _ = this._report.Body;
        }

        using ( this.SignIn( this._readerUser ) )
        {
            // The cache belongs to the execution context, not to the AsyncLocal value of the method, so the
            // decision made after the await is still cached when the method has returned.
            await ReadAsync();
            _ = this._report.Body;
        }

        Assert.Equal( 1, this._policy.Evaluations );
    }

    [Fact]
    public async Task ConcurrentTasks_ShareTheCache()
    {
        using ( this.SignIn( this._readerUser ) )
        {
            await Task.WhenAll( Task.Run( () => this._report.Body ), Task.Run( () => this._report.Body ) );

            var evaluations = this._policy.Evaluations;
            _ = this._report.Body;

            Assert.Equal( evaluations, this._policy.Evaluations );
        }
    }

    [Fact]
    public void Denial_IsCached_AndAuditedAtEachAccess()
    {
        var auditor = new CountingAuditor();

        using ( this.SignIn( this._summarizerOnly, auditor ) )
        {
            Assert.Throws<AccessDeniedException>( () => this._report.Body );
            Assert.Throws<AccessDeniedException>( () => this._report.Body );
        }

        Assert.Equal( 1, this._policy.Evaluations );
        Assert.Equal( 2, auditor.Denials );
    }

    [Fact]
    public void NewContext_StartsWithAnEmptyCache()
    {
        using ( this.SignIn( this._readerUser ) )
        {
            _ = this._report.Body;
        }

        using ( this.SignIn( this._readerUser ) )
        {
            _ = this._report.Body;
        }

        Assert.Equal( 2, this._policy.Evaluations );
    }

    [Fact]
    public void Grant_BreaksTheExecutionContext()
    {
        using ( this.SignIn( this._summarizerOnly ) )
        {
            // The parent context caches the denial.
            Assert.Throws<AccessDeniedException>( () => this._report.Body );

            // The grant starts a child context with an empty cache, so the cached denial does not apply to it.
            Assert.Equal( 17, this._report.GrantedBodyLength );

            // The decisions of the child context, which took the grant into account, are discarded with it.
            Assert.Throws<AccessDeniedException>( () => this._report.Body );
        }
    }

    [Fact]
    public void ChangeOfRights_AppliesToTheNextExecutionContext()
    {
        using ( this.SignIn( this._readerUser ) )
        {
            _ = this._report.Body;

            using ( SecurityContext.RunAsSystem() )
            {
                this._report.RevokeRole( this._readerUser, _reader );
            }

            // The cache is not invalidated: the decision stays valid until the execution context ends.
            _ = this._report.Body;
        }

        using ( this.SignIn( this._readerUser ) )
        {
            Assert.Throws<AccessDeniedException>( () => this._report.Body );
        }
    }

    [Fact]
    public void DefaultScope_DoesNothing()
    {
        var context = new UserSecurityContext( this._readerUser, this._policy );

        using ( SecurityContext.Use( context ) )
        {
            default(SecurityScope).Dispose();

            Assert.Same( context, SecurityContext.Current );
        }
    }

    private sealed class CountingAuditor : IAccessAuditor
    {
        public int Denials { get; private set; }

        public void OnAccessDenied( AccessDeniedException exception ) => this.Denials++;
    }
}
