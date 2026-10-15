// This test compiles the real configuration of the demo application (its base class, fabrics and permissions)
// with a small entity, to show the code that this configuration generates in the Sales namespace.

#if TEST_OPTIONS
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/Entity.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/User.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/RoleAssignmentRecord.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/AuthorizationFabric.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/DemoPermissions.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/Finance/FinanceFabric.cs)
#endif

using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Sales;

public sealed class Quote : Entity
{
    // Read and Write: the library defaults.
    public decimal Amount { get; set; }

    // Secured because the project fabric sets SecureNonPublicMembers.
    internal string? InternalNote { get; set; }

    // Requires Write because the project fabric sets DefaultExecutePermission.
    public void AddItem( string description ) { }

    [RequiresPermission( DemoPermissions.Approve )]
    public void Approve() { }

    // Not secured: the option set on the base member in the library is inherited by the override.
    protected override IRoleBasedSecurable? SecurityParent => null;

    public override string ToString() => "quote";
}
