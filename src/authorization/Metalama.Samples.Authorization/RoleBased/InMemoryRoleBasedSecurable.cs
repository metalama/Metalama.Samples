namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// A <see cref="RoleBasedSecurable"/> that stores its owner and role assignments in memory.
/// </summary>
public abstract class InMemoryRoleBasedSecurable : RoleBasedSecurable
{
    private readonly List<RoleAssignment> _roleAssignments = [];

    public override ISubject? Owner { get; set; }

    protected override IEnumerable<RoleAssignment> GetRoleAssignments() => this._roleAssignments;

    protected override void AddRoleAssignment( RoleAssignment assignment ) => this._roleAssignments.Add( assignment );

    protected override bool RemoveRoleAssignment( RoleAssignment assignment ) => this._roleAssignments.Remove( assignment );
}
