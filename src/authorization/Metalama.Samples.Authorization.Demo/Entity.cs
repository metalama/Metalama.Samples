using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// Base class of all business objects. The <see cref="ApplyDefaultPermissionsAttribute"/> aspect is inherited,
/// so every derived class gets permission checks on its members, configured by <see cref="AuthorizationOptions"/>.
/// </summary>
/// <remarks>
/// The class stores the owner and the role assignments in a form that Entity Framework can map:
/// a <see cref="User"/> navigation and a collection of <see cref="RoleAssignmentRecord"/>.
/// </remarks>
[ApplyDefaultPermissions]
public abstract class Entity : RoleBasedSecurable, IHasSecurityLabel
{
    /// <summary>
    /// Gets the label used in denial messages and audit records, for instance <c>Invoice #12</c>.
    /// It contains no protected data, so it is not secured.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public string SecurityLabel => $"{this.GetType().Name} #{this.Id}";

    /// <summary>
    /// Gets the database key. It identifies the object and is not confidential.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public int Id { get; private set; }

    // The aspect of the base property is inherited, so this override requires Read and ChangeOwnership.
    public override ISubject? Owner
    {
        get => this.OwnerUser;
        set
            => this.OwnerUser = value switch
            {
                null => null,
                User user => user,
                _ => throw new ArgumentException( $"The owner must be a {nameof(User)}.", nameof(value) )
            };
    }

    // The following members are storage, mapped by Entity Framework and used by the security infrastructure.
    // They are not secured: the public members that use them are.

    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected User? OwnerUser { get; set; }

    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected List<RoleAssignmentRecord> RoleAssignmentRecords { get; private set; } = [];

    protected override IEnumerable<RoleAssignment> GetRoleAssignments()
        => this.RoleAssignmentRecords.Select( r => r.ToRoleAssignment() );

    protected override void AddRoleAssignment( RoleAssignment assignment )
        => this.RoleAssignmentRecords.Add( new RoleAssignmentRecord( ToUser( assignment.Subject ), assignment.Role.Name ) );

    protected override bool RemoveRoleAssignment( RoleAssignment assignment )
        => this.RoleAssignmentRecords.RemoveAll( r => r.User.Equals( assignment.Subject ) && r.RoleName == assignment.Role.Name ) > 0;

    private static User ToUser( ISubject subject )
        => subject as User ?? throw new ArgumentException( $"Roles can only be assigned to a {nameof(User)}.", nameof(subject) );
}
