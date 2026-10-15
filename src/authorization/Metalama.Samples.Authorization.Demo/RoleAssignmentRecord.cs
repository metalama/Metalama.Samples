using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// The database representation of a <see cref="RoleAssignment"/>: a user and the name of a role.
/// Entity Framework stores it as an owned collection of each <see cref="Entity"/>.
/// </summary>
public sealed class RoleAssignmentRecord
{
    // Used by Entity Framework.
    private RoleAssignmentRecord()
    {
        this.User = null!;
        this.RoleName = null!;
    }

    public RoleAssignmentRecord( User user, string roleName )
    {
        this.User = user;
        this.RoleName = roleName;
    }

    public User User { get; private set; }

    public string RoleName { get; private set; }

    public RoleAssignment ToRoleAssignment() => new( this.User, new Role( this.RoleName ) );
}
