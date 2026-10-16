using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.AsyncMethod;
public class Document : ISecurable, IInitializable
{
  // The check runs at the start of the async method, so a denial faults the returned task.
  [RequiresPermission("Send")]
  public async Task SendAsync(CancellationToken cancellationToken = default)
  {
    AccessGuard.Demand(this, "Send", "Document.SendAsync", AccessKind.Execute);
    await Task.Delay(1, cancellationToken);
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