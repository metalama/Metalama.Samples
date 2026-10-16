using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.MethodDefaultPermission;
public class Document : ISecurable, IInitializable
{
  // Without arguments, a method requires the default execute permission, which is Write.
  [RequiresPermission]
  public void Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.Archive", AccessKind.Execute);
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
public class Report : ISecurable, IInitializable
{
  public void Refresh()
  {
    AccessGuard.Demand(this, "Write", "Report.Refresh", AccessKind.Execute);
  }
  // A method is excluded from default securing only explicitly.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  public string Preview() => "Preview";
  public Report([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Report() : this(context: default)
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