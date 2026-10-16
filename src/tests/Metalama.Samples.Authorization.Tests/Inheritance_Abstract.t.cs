using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Inheritance_Abstract;
public abstract class Document : ISecurable, IInitializable
{
  // An abstract member has no body to check, but the aspect is inherited by all overrides.
  [RequiresPermission("Approve")]
  public abstract void Approve();
  protected Document()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}
public class Contract : Document
{
  public override void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Contract.Approve", AccessKind.Execute);
  }
  public Contract()
  {
    SecurityContext.BeginInitialization(this);
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
}