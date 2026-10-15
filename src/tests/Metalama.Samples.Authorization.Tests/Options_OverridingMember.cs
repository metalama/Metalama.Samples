namespace Metalama.Samples.Authorization.Tests.Options_OverridingMember;

[SecurableType]
[AuthorizationOptions( SecureNonPublicMembers = true )]
public abstract class Entity
{
    // Options are inherited by overriding members, so the override in Invoice is not secured either.
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected abstract Entity? Parent { get; }

    protected abstract string Kind { get; }
}

// SecureNonPublicMembers is inherited from the base type.
public class Invoice : Entity
{
    protected override Entity? Parent => null;

    // Secured: the base member has no option.
    protected override string Kind => "Invoice";
}
