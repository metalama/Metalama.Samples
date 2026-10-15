using Xunit;

namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// Runs every operation of <see cref="PermissionMatrix"/> as every actor, on a fresh organization each time.
/// </summary>
public class PermissionMatrixTests
{
    public static TheoryData<string, Actor, bool> Cases
    {
        get
        {
            var data = new TheoryData<string, Actor, bool>();

            foreach ( var operation in PermissionMatrix.Operations )
            {
                foreach ( var actor in ActorExtensions.All )
                {
                    data.Add( operation.Name, actor, operation.AllowedActors.Contains( actor ) );
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData( nameof(Cases) )]
    public async Task Operation_IsAllowedOnlyToExpectedActors( string operation, Actor actor, bool isAllowed )
    {
        var org = DemoOrganization.Create();
        var testedOperation = PermissionMatrix.Get( operation );

        if ( testedOperation.Arrange != null )
        {
            using ( SecurityContext.RunAsSystem() )
            {
                await testedOperation.Arrange( org );
            }
        }

        var user = org.GetUser( actor );

        using ( org.SignIn( user ) )
        {
            if ( isAllowed )
            {
                // Any exception fails the test, including a business-rule exception, which would mean
                // that the row tests a business rule instead of a permission.
                await testedOperation.Act( org );
            }
            else
            {
                var exception = await Assert.ThrowsAsync<AccessDeniedException>( () => testedOperation.Act( org ) );

                Assert.Equal( user, exception.Subject );
            }
        }
    }

    [Fact]
    public void OperationNames_AreUnique()
    {
        var duplicates = PermissionMatrix.Operations.GroupBy( o => o.Name ).Where( g => g.Count() > 1 ).Select( g => g.Key );

        Assert.Empty( duplicates );
    }

    [Fact]
    public void EveryActorExceptVera_HasAPermissionBeyondTheDirectory()
    {
        var everyoneOperations = PermissionMatrix.Operations
            .Where( o => o.AllowedActors.Count == ActorExtensions.All.Count )
            .ToHashSet();

        var actorsWithOwnPermissions = PermissionMatrix.Operations
            .Except( everyoneOperations )
            .SelectMany( o => o.AllowedActors )
            .ToHashSet();

        Assert.Equal( ActorExtensions.All.Where( a => a != Actor.Vera ), ActorExtensions.All.Where( actorsWithOwnPermissions.Contains ) );
    }
}
