using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// A <see cref="RoleBasedSecurable"/> that stores its owner and role assignments with Entity Framework: a
/// <see cref="User"/> navigation and an owned collection of <see cref="RoleAssignmentRecord"/>.
/// </summary>
/// <remarks>
/// The storage members are excluded from default securing: the security infrastructure calls them, and the public
/// members that use them are secured. Map each class with <see cref="EfRoleBasedSecurableMapping.ConfigureRoleBasedStorage{T}"/>.
/// </remarks>
public abstract class EfRoleBasedSecurable : RoleBasedSecurable
{
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

    // The names of the storage members, for the mapping (see EfRoleBasedSecurableMapping).
    internal const string OwnerUserMember = nameof(OwnerUser);
    internal const string RoleAssignmentRecordsMember = nameof(RoleAssignmentRecords);

    private static User ToUser( ISubject subject )
        => subject as User ?? throw new ArgumentException( $"Roles can only be assigned to a {nameof(User)}.", nameof(subject) );
}
