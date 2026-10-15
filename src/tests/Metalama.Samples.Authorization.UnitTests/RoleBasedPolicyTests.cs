using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

public class RoleBasedPolicyTests
{
    private readonly TestUser _alice = new( "alice" );
    private readonly TestUser _bob = new( "bob" );

    // The policy reads secured members such as Owner, so it is called in a system scope, as AccessGuard does.
    private static bool IsGranted( IAuthorizationPolicy policy, ISubject subject, Permission permission, object securable )
    {
        using ( SecurityContext.RunAsSystem() )
        {
            return policy.IsGranted( subject, permission, securable );
        }
    }

    private Folder CreateFolder( Folder? parent = null, Role? aliceRole = null )
    {
        using ( SecurityContext.RunAsSystem() )
        {
            var folder = new Folder { Parent = parent };

            if ( aliceRole != null )
            {
                folder.AssignRole( this._alice, aliceRole );
            }

            return folder;
        }
    }

    [Fact]
    public void NoMatchingRule_IsDenied()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Write, TestRoles.Editor );

        Assert.False( IsGranted( policy, this._alice, Write, this.CreateFolder() ) );
    }

    [Fact]
    public void AllowRule_GrantsMembersOfTheRole()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Write, TestRoles.Editor );
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        Assert.True( IsGranted( policy, this._alice, Write, folder ) );
        Assert.False( IsGranted( policy, this._bob, Write, folder ) );
    }

    [Fact]
    public void AllowRule_DoesNotGrantOtherPermissions()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Write, TestRoles.Editor );

        Assert.False( IsGranted( policy, this._alice, ManageRoles, this.CreateFolder( aliceRole: TestRoles.Editor ) ) );
    }

    [Fact]
    public void DenyRule_OverridesAllowRule()
    {
        var policy = new RoleBasedAuthorizationPolicy()
            .Allow<Folder>( Write, TestRoles.Editor )
            .Deny<object>( Write, TestRoles.Reviewer );

        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.RunAsSystem() )
        {
            folder.AssignRole( this._alice, TestRoles.Reviewer );
        }

        Assert.False( IsGranted( policy, this._alice, Write, folder ) );
    }

    [Fact]
    public void RuleOnBaseType_AppliesToDerivedType()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<object>( Read, Role.Everyone );

        Assert.True( IsGranted( policy, this._alice, Read, new SpecialFolder() ) );
    }

    [Fact]
    public void RuleOnDerivedType_DoesNotApplyToBaseType()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<SpecialFolder>( Read, Role.Everyone );

        Assert.False( IsGranted( policy, this._alice, Read, new Folder() ) );
        Assert.True( IsGranted( policy, this._alice, Read, new SpecialFolder() ) );
    }

    [Fact]
    public void Everyone_IsImplicit()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Read, Role.Everyone );

        Assert.True( IsGranted( policy, new TestUser( "stranger" ), Read, new Folder() ) );
    }

    [Fact]
    public void Owner_IsComputedFromTheObject()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Write, Role.Owner );
        var folder = this.CreateFolder();

        using ( SecurityContext.RunAsSystem() )
        {
            folder.Owner = this._alice;
        }

        Assert.True( IsGranted( policy, this._alice, Write, folder ) );
        Assert.False( IsGranted( policy, this._bob, Write, folder ) );
    }

    [Fact]
    public void Owner_IsNotInheritedByChildren()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Write, Role.Owner );
        var parent = this.CreateFolder();

        using ( SecurityContext.RunAsSystem() )
        {
            parent.Owner = this._alice;
        }

        Assert.False( IsGranted( policy, this._alice, Write, this.CreateFolder( parent ) ) );
    }

    [Fact]
    public void AssignedRole_IsInheritedThroughSecurityParent()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Write, TestRoles.Editor );
        var root = this.CreateFolder( aliceRole: TestRoles.Editor );
        var grandChild = this.CreateFolder( this.CreateFolder( root ) );

        Assert.True( IsGranted( policy, this._alice, Write, grandChild ) );
    }

    [Fact]
    public void NonSecurableObject_IsDenied()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<object>( Read, Role.Everyone );

        Assert.False( IsGranted( policy, this._alice, Read, new NotSecurable() ) );
    }

    [Fact]
    public void RevokedRole_NoLongerGrants()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( Write, TestRoles.Editor );
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.True( folder.RevokeRole( this._alice, TestRoles.Editor ) );
            Assert.False( folder.RevokeRole( this._alice, TestRoles.Editor ) );
        }

        Assert.False( IsGranted( policy, this._alice, Write, folder ) );
    }

    [Fact]
    public void BuiltInRoles_CannotBeAssigned()
    {
        var folder = this.CreateFolder();

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Throws<ArgumentException>( () => folder.AssignRole( this._alice, Role.Owner ) );
            Assert.Throws<ArgumentException>( () => folder.AssignRole( this._alice, Role.Everyone ) );
        }
    }

    [Fact]
    public void AssigningRoles_RequiresManageRoles()
    {
        var policy = new RoleBasedAuthorizationPolicy().Allow<Folder>( ManageRoles, TestRoles.Editor );
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.Use( new UserSecurityContext( this._bob, policy ) ) )
        {
            Assert.Throws<AccessDeniedException>( () => folder.AssignRole( this._bob, TestRoles.Editor ) );
        }

        using ( SecurityContext.Use( new UserSecurityContext( this._alice, policy ) ) )
        {
            folder.AssignRole( this._bob, TestRoles.Reviewer );
        }
    }
}
