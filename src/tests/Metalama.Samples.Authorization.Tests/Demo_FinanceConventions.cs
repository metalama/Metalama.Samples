// This test compiles the real configuration of the demo application (its base class, fabrics and permissions)
// with a small entity, to show the code that this configuration generates in the Finance namespace.

#if TEST_OPTIONS
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/Entity.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/AuthorizationFabric.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/DemoPermissions.cs)
// @Include(../../authorization/Metalama.Samples.Authorization.Demo/Finance/FinanceFabric.cs)
#endif

using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Finance;

public sealed class Forecast : Entity
{
    // Reading requires ReadSensitive, from the namespace fabric. Writing keeps the default.
    public decimal Amount { get; set; }

    // Requires Write, from the project fabric.
    public void Revise( decimal amount ) => this.Amount = amount;

    protected override IRoleBasedSecurable? SecurityParent => null;
}
