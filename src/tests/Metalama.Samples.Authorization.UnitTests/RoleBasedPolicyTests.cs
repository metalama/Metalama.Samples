using Metalama.Samples.Authorization.RoleBased;
using Xunit;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.UnitTests;

public class RoleBasedPolicyTests
{
    private readonly TestUser _alice = new( "alice" );
    private readonly TestUser _bob = new( "bob" );

    // The policy reads secured members such as Owner, so it is called in a system scope, as AccessGuard does.
    private static bool IsGranted( IAuthorizationPolicy policy, ISubject subject, Permission permission, ISecurable securable )
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
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, TestRoles.Editor ).Build();

        Assert.False( IsGranted( policy, this._alice, Write, this.CreateFolder() ) );
    }

    [Fact]
    public void AllowRule_GrantsMembersOfTheRole()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, TestRoles.Editor ).Build();
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        Assert.True( IsGranted( policy, this._alice, Write, folder ) );
        Assert.False( IsGranted( policy, this._bob, Write, folder ) );
    }

    [Fact]
    public void AllowRule_DoesNotGrantOtherPermissions()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, TestRoles.Editor ).Build();

        Assert.False( IsGranted( policy, this._alice, ManageRoles, this.CreateFolder( aliceRole: TestRoles.Editor ) ) );
    }

    [Fact]
    public void DenyRule_OverridesAllowRule()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder()
            .Allow<Folder>( Write, TestRoles.Editor )
            .Deny<object>( Write, TestRoles.Auditor ).Build();

        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.RunAsSystem() )
        {
            folder.AssignRole( this._alice, TestRoles.Auditor );
        }

        Assert.False( IsGranted( policy, this._alice, Write, folder ) );
    }

    [Fact]
    public void RuleOnBaseType_AppliesToDerivedType()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<object>( Read, Role.Everyone ).Build();

        Assert.True( IsGranted( policy, this._alice, Read, new SpecialFolder() ) );
    }

    [Fact]
    public void RuleOnDerivedType_DoesNotApplyToBaseType()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<SpecialFolder>( Read, Role.Everyone ).Build();

        Assert.False( IsGranted( policy, this._alice, Read, new Folder() ) );
        Assert.True( IsGranted( policy, this._alice, Read, new SpecialFolder() ) );
    }

    [Fact]
    public void Everyone_IsImplicit()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Read, Role.Everyone ).Build();

        Assert.True( IsGranted( policy, new TestUser( "stranger" ), Read, new Folder() ) );
    }

    [Fact]
    public void Owner_IsComputedFromTheObject()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, Role.Owner ).Build();
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
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, Role.Owner ).Build();
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
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, TestRoles.Editor ).Build();
        var root = this.CreateFolder( aliceRole: TestRoles.Editor );
        var grandChild = this.CreateFolder( this.CreateFolder( root ) );

        Assert.True( IsGranted( policy, this._alice, Write, grandChild ) );
    }

    [Fact]
    public void ObjectWithoutRoles_IsDenied()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<object>( Read, Role.Everyone ).Build();

        Assert.False( IsGranted( policy, this._alice, Read, new NotRoleBased() ) );
    }

    [Fact]
    public void RevokedRole_NoLongerGrants()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, TestRoles.Editor ).Build();
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.True( folder.RevokeRole( this._alice, TestRoles.Editor ) );
            Assert.False( folder.RevokeRole( this._alice, TestRoles.Editor ) );
        }

        Assert.False( IsGranted( policy, this._alice, Write, folder ) );
    }

    [Fact]
    public void ComputedRoles_CannotBeAssigned()
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
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( ManageRoles, TestRoles.Editor ).Build();
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.Use( new UserSecurityContext( this._bob, policy ) ) )
        {
            Assert.Throws<AccessDeniedException>( () => folder.AssignRole( this._bob, TestRoles.Editor ) );
        }

        using ( SecurityContext.Use( new UserSecurityContext( this._alice, policy ) ) )
        {
            folder.AssignRole( this._bob, TestRoles.Auditor );
        }
    }

    [Fact]
    public void ApplicationComputedRole_IsResolvedByTheObject_AndCannotBeAssigned()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Deny<Folder>( Write, TestRoles.Reviewer ).Allow<Folder>( Write, Role.Everyone ).Build();
        var folder = new ReviewedFolder { Reviewer = this._alice };

        Assert.False( IsGranted( policy, this._alice, Write, folder ) );
        Assert.True( IsGranted( policy, this._bob, Write, folder ) );

        // A computed role of a parent is not inherited.
        Assert.True( IsGranted( policy, this._alice, Write, this.CreateFolder( folder ) ) );

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Throws<ArgumentException>( () => folder.AssignRole( this._bob, TestRoles.Reviewer ) );
        }
    }

    [Fact]
    public void AssignedRole_IsInheritedFromAParentThatOnlyImplementsTheInterface()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( Write, TestRoles.Editor ).Build();
        var folder = this.CreateFolder( new Folder { Parent = new ExternalSecurable( this._alice ) } );

        Assert.True( IsGranted( policy, this._alice, Write, folder ) );
        Assert.False( IsGranted( policy, this._bob, Write, folder ) );
    }

    [Fact]
    public void AssigningARoleToOneself_IsDenied()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( ManageRoles, TestRoles.Editor ).Build();
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.Use( new UserSecurityContext( this._alice, policy ) ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => folder.AssignRole( this._alice, TestRoles.Auditor ) );

            Assert.Contains( "cannot assign a role to themself", exception.Message, StringComparison.Ordinal );

            // Assigning the same role to someone else is allowed.
            folder.AssignRole( this._bob, TestRoles.Auditor );
        }
    }

    [Fact]
    public void RevokingARoleFromOneself_IsDenied()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( ManageRoles, TestRoles.Editor ).Build();
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.RunAsSystem() )
        {
            folder.AssignRole( this._alice, TestRoles.Auditor );
            folder.AssignRole( this._bob, TestRoles.Auditor );
        }

        using ( SecurityContext.Use( new UserSecurityContext( this._alice, policy ) ) )
        {
            // The revoked role could carry Deny rules.
            var exception = Assert.Throws<AccessDeniedException>( () => folder.RevokeRole( this._alice, TestRoles.Auditor ) );

            Assert.Contains( "cannot revoke a role from themself", exception.Message, StringComparison.Ordinal );

            Assert.True( folder.RevokeRole( this._bob, TestRoles.Auditor ) );
        }
    }

    [Fact]
    public void MakingOneselfTheOwner_IsDenied()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Allow<Folder>( ChangeOwnership, TestRoles.Editor ).Build();
        var folder = this.CreateFolder( aliceRole: TestRoles.Editor );

        using ( SecurityContext.Use( new UserSecurityContext( this._alice, policy ) ) )
        {
            var exception = Assert.Throws<AccessDeniedException>( () => folder.Owner = this._alice );

            Assert.Contains( "cannot make themself the owner", exception.Message, StringComparison.Ordinal );

            // Giving the object to someone else is allowed.
            folder.Owner = this._bob;
        }
    }

    [Fact]
    public void CreatorOfAnObject_CanMakeThemselfTheOwner()
    {
        var policy = new RoleBasedAuthorizationPolicyBuilder().Build();
        Folder folder;

        using ( SecurityContext.Use( new UserSecurityContext( this._alice, policy ) ) )
        {
            // The object is being initialized, so the creator has unlimited rights on it, and can own it.
            folder = new Folder { Owner = this._alice };

            // After the initialization, changing the owner requires ChangeOwnership.
            Assert.Throws<AccessDeniedException>( () => folder.Owner = this._bob );
        }

        using ( SecurityContext.RunAsSystem() )
        {
            Assert.Equal( this._alice, folder.Owner );
        }
    }

    [Fact]
    public void RuleOnAnInterface_IsRejected()
        => Assert.Throws<ArgumentException>( () => new RoleBasedAuthorizationPolicyBuilder().Allow<IFolder>( Read, Role.Everyone ) );

    [Fact]
    public void Builder_ChangesAfterBuild_DoNotAffectThePolicy()
    {
        var builder = new RoleBasedAuthorizationPolicyBuilder();
        var policy = builder.Build();

        builder.Allow<Folder>( Read, Role.Everyone );

        Assert.False( IsGranted( policy, this._alice, Read, new Folder() ) );
        Assert.True( IsGranted( builder.Build(), this._alice, Read, new Folder() ) );
    }
}
