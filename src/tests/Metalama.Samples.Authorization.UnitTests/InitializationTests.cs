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

    [Fact]
    public void RootClass_KeepsItsParameterlessConstructor()
    {
        // NotRoleBased implements ISecurable directly, so it is the first class of its hierarchy that receives the
        // initializers. Reflection and the new() constraint need its parameterless constructor.
        Assert.NotNull( Activator.CreateInstance<NotRoleBased>() );
        Assert.NotNull( Create<NotRoleBased>() );
    }

    private static T Create<T>()
        where T : new()
        => new();

    [Fact]
    public void ObjectCreatedAtARewrittenCallSite_IsInitialized()
    {
        using ( SignIn( this._userWithoutRights ) )
        {
            ISecurable report = new Report( "Draft" );

            Assert.True( report.IsInitialized );
        }
    }

    [Fact]
    public void ObjectWhoseInitializationDidNotEnd_IsUnusable()
    {
        Report report;

        using ( SignIn( this._userWithoutRights ) )
        {
            // Metalama cannot rewrite a call through reflection, so Initialize is not called.
            report = Activator.CreateInstance<Report>();

            Assert.False( ((ISecurable) report).IsInitialized );

            // The creator can still set the properties, as an object initializer does, but cannot read the object
            // or call its methods.
            report.Body = "Changed";
            Assert.Throws<InvalidOperationException>( () => report.Body );
            Assert.Throws<InvalidOperationException>( () => report.Publish() );
        }

        // Another execution context cannot use the object at all, even to set a property.
        using ( SignIn( this._readerUser ) )
        {
            Assert.Throws<InvalidOperationException>( () => report.Body = "Changed again" );
        }
    }

    [Fact]
    public void CompleteInitialization_MakesTheObjectUsable()
    {
        using ( SignIn( this._userWithoutRights ) )
        {
            var report = Activator.CreateInstance<Report>();
            report.Body = "Draft";

            ((ISecurable) report).CompleteInitialization();

            Assert.True( ((ISecurable) report).IsInitialized );

            // The object is checked like any other.
            Assert.Throws<AccessDeniedException>( () => report.Body );
        }
    }
}
