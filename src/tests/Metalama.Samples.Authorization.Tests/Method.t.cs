using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Method;
public class Document : ISecurable, IInitializable
{
  [RequiresPermission("Approve")]
  public void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Document.Approve", AccessKind.Execute);
  }
  [RequiresPermission("Read")]
  public string Render(string format)
  {
    AccessGuard.Demand(this, "Read", "Document.Render", AccessKind.Execute);
    return $"Document as {format}";
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