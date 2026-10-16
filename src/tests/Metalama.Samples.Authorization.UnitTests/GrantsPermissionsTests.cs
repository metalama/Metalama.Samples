using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

/// <summary>
/// A member with <see cref="GrantsPermissionsAttribute"/> grants permissions on its own instance while it runs,
/// after its own check has passed. The grant does not apply to other objects, to other contexts, or after the member
/// has returned.
/// </summary>
public class GrantsPermissionsTests
{
    private static readonly Role _summarizer = new( "Summarizer" );
    private static readonly Role _publisher = new( "Publisher" );
    private static readonly Role _redacted = new( "Redacted" );

    private static readonly RoleBasedAuthorizationPolicy _policy = new RoleBasedAuthorizationPolicyBuilder()
        .Allow<Report>( "Summarize", _summarizer )
        .Allow<Report>( "Publish", _publisher )
        .Deny<Report>( Read, _redacted )
        .Build();

    private readonly TestUser _summarizerOnly = new( "summarizer" );
    private readonly TestUser _publisherOnly = new( "publisher" );
    private readonly TestUser _redactedSummarizer = new( "redacted summarizer" );
    private readonly Report _report;
    private readonly Report _otherReport;

    public GrantsPermissionsTests()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            this._report = new Report { Body = "Quarterly results" };
            this._otherReport = new Report { Body = "Annual results" };

            foreach ( var report in new[] { this._report, this._otherReport } )
            {
                report.AssignRole( this._summarizerOnly, _summarizer );
                report.AssignRole( this._publisherOnly, _publisher );
                report.AssignRole( this._redactedSummarizer, _summarizer );
                report.AssignRole( this._redactedSummarizer, _redacted );
            }
        }
    }

    private static IDisposable SignIn( ISubject subject ) => SecurityContext.Use( Context( subject ) );

    private static UserSecurityContext Context( ISubject subject ) => new( subject, _policy );

    [Fact]
    public void Aggregate_ThatGrantsTheDetailPermission_RequiresOnlyItsOwnPermission()
    {
        using ( SignIn( this._summarizerOnly ) )
        {
            Assert.Equal( 17, this._report.GrantedBodyLength );
        }
    }

    [Fact]
    public void Grant_EndsWhenTheMemberReturns()
    {
        using ( SignIn( this._summarizerOnly ) )
        {
            _ = this._report.GrantedBodyLength;

            var exception = Assert.Throws<AccessDeniedException>( () => this._report.Body );
            Assert.Equal( "Report.Body", exception.Member );
        }
    }

    [Fact]
    public void Grant_DoesNotSatisfyTheCheckOfItsOwnMember()
    {
        using ( SignIn( this._publisherOnly ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => this._report.SelfGranted );
            Assert.Equal( "Report.SelfGranted", exception.Member );
        }
    }

    [Fact]
    public void Grant_RequiresTheCheckOfTheMemberToPass()
    {
        // The publisher does not have Summarize, so the member is denied before the grant.
        using ( SignIn( this._publisherOnly ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => this._report.GrantedBodyLength );
            Assert.Equal( "Report.GrantedBodyLength", exception.Member );
        }
    }

    [Fact]
    public void Grant_AppliesToTheCurrentInstanceOnly()
    {
        using ( SignIn( this._summarizerOnly ) )
        {
            // CompareLength reads this.Body, which is granted, then other.Body, which is not.
            var exception = Assert.Throws<AccessDeniedException>( () => this._report.CompareLength( this._otherReport ) );

            Assert.Equal( "Report.Body", exception.Member );
        }
    }

    [Fact]
    public void Grant_DoesNotApplyToAnotherContext()
    {
        using ( SignIn( this._summarizerOnly ) )
        {
            // The member switches to the context of the publisher, who does not get the grant of the summarizer.
            var exception = Assert.Throws<AccessDeniedException>( () => this._report.ReadBodyAs( Context( this._publisherOnly ) ) );

            Assert.Equal( this._publisherOnly, exception.Subject );
        }
    }

    [Fact]
    public async Task Grant_FlowsIntoTheContinuationsOfAnAsyncMember()
    {
        using ( SignIn( this._summarizerOnly ) )
        {
            Assert.Equal( 17, await this._report.GetBodyLengthAsync() );

            // The caller does not get the grant of the async member.
            Assert.Throws<AccessDeniedException>( () => this._report.Body );
        }
    }

    [Fact]
    public void Grant_TakesPrecedenceOverADenialOfThePolicy()
    {
        using ( SignIn( this._redactedSummarizer ) )
        {
            Assert.Throws<AccessDeniedException>( () => this._report.Body );
            Assert.Equal( 17, this._report.GrantedBodyLength );
        }
    }

    [Fact]
    public void Constructor_ThatGrantsWrite_CanSetThePropertyOfTheNewObject()
    {
        using ( SignIn( this._summarizerOnly ) )
        {
            // Without the grant, setting the property of the new object is denied.
            Assert.Throws<AccessDeniedException>( () => new Report { Body = "Draft" } );

            var report = new Report( "Draft" );

            // The grant ends when the constructor returns.
            Assert.Throws<AccessDeniedException>( () => report.Body = "Changed" );
            Assert.Throws<AccessDeniedException>( () => this._report.Body = "Changed" );
        }
    }

    [Fact]
    public void Method_ThatGrantsReadAndWrite_CanChangeTheProperty()
    {
        using ( SignIn( this._publisherOnly ) )
        {
            this._report.PublishWithGrant();
        }

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( "Quarterly results (published)", this._report.Body );
        }
    }
}
