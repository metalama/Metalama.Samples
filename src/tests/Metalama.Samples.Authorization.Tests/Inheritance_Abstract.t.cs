using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Inheritance_Abstract;
public abstract class Document : ISecurable, IInitializable
{
  // An abstract member has no body to check, but the aspect is inherited by all overrides.
  [RequiresPermission("Approve")]
  public abstract void Approve();
  protected Document([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  protected Document() : this(context: default)
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
public class Contract : Document
{
  public override void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Contract.Approve", AccessKind.Execute);
  }
  public Contract([AspectGenerated] InitializationContext context) : base(context.Descend(InitializationSlot.OnConstructed))
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Contract() : this(context: default)
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