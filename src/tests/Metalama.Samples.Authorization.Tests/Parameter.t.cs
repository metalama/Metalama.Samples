using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Parameter;
public class Folder : ISecurable, IInitializable
{
  public Folder([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Folder() : this(context: default)
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
public class Document
{
  // Only the attributed parameter is checked. The permission is required on the argument, not on this.
  public void MoveTo([RequiresPermission("Write")] Folder target, string? comment)
  {
    ISecurable? argument = target;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "Write", "Document.MoveTo(target)", AccessKind.Execute);
    }
  }
}