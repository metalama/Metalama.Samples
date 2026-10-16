using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Options_OverridingMember;
[AuthorizationOptions(SecureNonPublicMembers = true)]
public abstract class Entity : ISecurable, IInitializable
{
  // Options are inherited by overriding members, so the override in Invoice is not secured either.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  protected abstract Entity? Parent { get; }
  protected abstract string Kind { get; }
  protected Entity()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}
// SecureNonPublicMembers is inherited from the base type.
public class Invoice : Entity
{
  protected override Entity? Parent => null;
  // Secured: the base member has no option.
  protected override string Kind
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Invoice.Kind", AccessKind.Read);
      return "Invoice";
    }
  }
  public Invoice()
  {
    SecurityContext.BeginInitialization(this);
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
}