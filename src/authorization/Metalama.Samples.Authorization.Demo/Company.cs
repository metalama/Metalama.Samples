using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// The root of the organization. Roles assigned on the company apply to all its departments and their objects.
/// </summary>
public sealed class Company( string name ) : Entity
{
    // Init-only: the application cannot rename the company, whatever the rules of the policy.
    public string Name { get; init; } = name;

    protected override IRoleBasedSecurable? SecurityParent => null;

    public override string ToString() => this.Name;
}
