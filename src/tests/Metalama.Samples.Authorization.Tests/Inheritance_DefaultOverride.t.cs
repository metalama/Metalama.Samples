using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Inheritance_DefaultOverride;
[AuthorizationOptions(DefaultExecutePermission = "Write")]
public class Document : ISecurable, IInitializable
{
  // Secured by default.
  public virtual void Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.Archive", AccessKind.Execute);
  }
  // Secured explicitly with another permission.
  [RequiresPermission("Approve")]
  public virtual void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Document.Approve", AccessKind.Execute);
  }
  public Document([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Document() : this(context: default)
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
// Each override gets exactly one check, with the permission of the base member.
public class Contract : Document
{
  public override void Archive()
  {
    AccessGuard.Demand(this, "Write", "Contract.Archive", AccessKind.Execute);
  }
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