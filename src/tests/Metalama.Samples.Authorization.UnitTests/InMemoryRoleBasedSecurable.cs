using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.UnitTests;

/// <summary>
/// A <see cref="RoleBasedSecurable"/> that stores its owner and role assignments in memory, for the tests of the
/// framework. The demo application stores them with Entity Framework (see <c>EfRoleBasedSecurable</c>).
/// </summary>
public abstract class InMemoryRoleBasedSecurable : RoleBasedSecurable
{
    private readonly List<RoleAssignment> _roleAssignments = [];

    public override ISubject? Owner { get; set; }

    protected override IEnumerable<RoleAssignment> GetRoleAssignments() => this._roleAssignments;

    protected override void AddRoleAssignment( RoleAssignment assignment ) => this._roleAssignments.Add( assignment );

    protected override bool RemoveRoleAssignment( RoleAssignment assignment ) => this._roleAssignments.Remove( assignment );
}
