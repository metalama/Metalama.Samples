using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

/// <summary>
/// The caller of a constructor has unlimited rights on the new object until its initialization ends, after the object
/// initializer. The accesses to other objects are checked.
/// </summary>
public class InitializationTests
{
    private static readonly Role _reader = new( "Reader" );

    private static readonly RoleBasedAuthorizationPolicy _policy =
        new RoleBasedAuthorizationPolicyBuilder().Allow<Report>( Read, _reader ).Build();

    private readonly TestUser _userWithoutRights = new( "nobody" );
    private readonly TestUser _readerUser = new( "reader" );
    private readonly Report _source;

    public InitializationTests()
    {
        using ( SecurityContext.RunAsSystem() )
        {
            this._source = new Report { Body = "Quarterly results" };
            this._source.AssignRole( this._readerUser, _reader );
        }
    }

    private static SecurityScope SignIn( ISubject subject ) => SecurityContext.Use( new UserSecurityContext( subject, _policy ) );

    [Fact]
    public void Constructor_SetsSecuredMembers_WithoutPermission()
    {
        using ( SignIn( this._userWithoutRights ) )
        {
            var report = new Report( "Draft" );

            // The initialization has ended: the new object is checked like any other.
            Assert.Throws<AccessDeniedException>( () => report.Body );
        }
    }

    [Fact]
    public void ObjectInitializer_SetsSecuredMembers_WithoutPermission()
    {
        using ( SignIn( this._userWithoutRights ) )
        {
            var report = new Report { Body = "Draft" };

            Assert.Throws<AccessDeniedException>( () => report.Body = "Changed" );
        }
    }

    [Fact]
    public void Constructor_IsCheckedOnOtherObjects()
    {
        using ( SignIn( this._userWithoutRights ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => new Report( this._source ) );

            Assert.Equal( "Report.Body", exception.Member );
        }

        using ( SignIn( this._readerUser ) )
        {
            _ = new Report( this._source );
        }
    }

    [Fact]
    public void WithoutContext_ConstructorFailsClosed()
        => Assert.Throws<InvalidOperationException>( () => new Report( "Draft" ) );
}
