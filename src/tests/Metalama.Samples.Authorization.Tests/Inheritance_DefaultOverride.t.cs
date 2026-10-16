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
  public Document()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
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