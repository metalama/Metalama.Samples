using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Options_OverridingMember;
[AuthorizationOptions(SecureNonPublicMembers = true)]
public abstract class Entity : ISecurable, IInitializable
{
  // Options are inherited by overriding members, so the override in Invoice is not secured either.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  protected abstract Entity? Parent { get; }
  protected abstract string Kind { get; }
  protected Entity([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  protected Entity() : this(context: default)
  {
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
  protected virtual void OnConstructed(InitializationContext context = default)
  {
    SecurityContext.EndConstruction(this);
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
  public Invoice([AspectGenerated] InitializationContext context) : base(context.Descend(InitializationSlot.OnConstructed))
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Invoice() : this(context: default)
  {
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
  protected override void OnConstructed(InitializationContext context = default)
  {
    base.OnConstructed(context);
    SecurityContext.EndConstruction(this);
  }
}