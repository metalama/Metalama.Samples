using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Parameter;
public class Folder : ISecurable, IInitializable
{
  public Folder()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
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